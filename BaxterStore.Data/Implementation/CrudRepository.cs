﻿using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaxterStore.Data.Implementation
{
    public abstract class CrudRepository<TDataEntity> : ICrudRepository<TDataEntity> where TDataEntity : DataEntity
    {
        private readonly DatabaseConfiguration _databaseConfiguration;
        private readonly TableConfiguration _tableConfiguration;
        private readonly ICacheHandler<TDataEntity> _cacheHandler;

        public CrudRepository(DatabaseConfiguration databaseConfiguration, TableConfiguration tableConfiguration, ICacheHandler<TDataEntity> cacheHandler)
        {
            _databaseConfiguration = databaseConfiguration ?? throw new ArgumentNullException(nameof(databaseConfiguration));
            _tableConfiguration = tableConfiguration ?? throw new ArgumentNullException(nameof(tableConfiguration));
            _cacheHandler = cacheHandler ?? throw new ArgumentNullException(nameof(cacheHandler));
        }

        public async Task<TDataEntity> Add(TDataEntity dataEntity)
        {
            if (dataEntity is null) throw new ArgumentNullException(nameof(dataEntity));
            if (string.IsNullOrWhiteSpace(dataEntity.Id)) dataEntity.Id = Guid.NewGuid().ToString();

            var command = BuildAddCommand(dataEntity);

            using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString))
            {
                var result = await connection.QuerySingleAsync<TDataEntity>(command, dataEntity);

                _cacheHandler.AddToCache(dataEntity);

                return result;
            }
        }

        public async Task Delete(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            var command = BuildDeleteStatement();

            _cacheHandler.DropFromCache(entityId);

            using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString))
            {
                await connection.ExecuteAsync(command, new { Id = entityId });
            }
        }

        public async Task<TDataEntity> FindById(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            if (_cacheHandler.TryGetValue(entityId, out TDataEntity dataEntity)) return dataEntity;

            var command = BuildFindByIdQuery();

            using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString))
            {
                var result = await connection.QuerySingleAsync<TDataEntity>(command, new { Id = entityId });

                _cacheHandler.AddToCache(result);

                return result;
            }
        }

        public async Task<IEnumerable<TDataEntity>> Search(IEnumerable<SearchParameter> searchParameters)
        {
            if (!searchParameters?.Any() ?? false) throw new ArgumentNullException(nameof(searchParameters));

            var command = BuildSearchQuery(searchParameters);
            var dynamicDapperParameters = new DynamicParameters();

            foreach (var parameter in searchParameters)
            {
                dynamicDapperParameters.Add(parameter.Column, parameter.Value);
            }

            using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString))
            {
                return await connection.QueryAsync<TDataEntity>(command, dynamicDapperParameters);
            }
        }

        public async Task<TDataEntity> Update(TDataEntity dataEntity)
        {
            if (dataEntity is null) throw new ArgumentNullException(nameof(dataEntity));

            var command = BuildUpdateCommand(dataEntity);

            using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString))
            {
                var updatedDataEntity = await connection.QuerySingleAsync<TDataEntity>(command, dataEntity);

                _cacheHandler.UpdateExisting(updatedDataEntity);

                return updatedDataEntity;
            }
        }

        private PropertyInfo[] GetPropertyInfo(TDataEntity dataEntity) => dataEntity.GetType().GetProperties();

        private string BuildUpdateCommand(TDataEntity dataEntity)
        {
            var properties = GetPropertyInfo(dataEntity);
            var output = string.Join(",", properties.Select(x => $"INSERTED.{x.Name}"));

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("UPDATE ");
            stringBuilder.Append(_tableConfiguration.Table);

            stringBuilder.Append(" SET ");
            var sets = string.Join(",", properties.Select(x => $"{x.Name} = @{x.Name}"));
            stringBuilder.Append(sets);

            stringBuilder.Append(" OUTPUT ");
            stringBuilder.Append(output);

            stringBuilder.Append(" WHERE [Id] = @Id");

            return stringBuilder.ToString();
        }

        private string BuildSearchQuery(IEnumerable<SearchParameter> searchParameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT * FROM ");
            stringBuilder.Append(_tableConfiguration.Table);
            stringBuilder.Append(" WHERE ");

            var searchArgs = string.Join(" AND ", searchParameters.Select(x => $"[{x.Column}] = @{x.Column}"));
            stringBuilder.Append(searchArgs);

            return stringBuilder.ToString();
        }

        private string BuildFindByIdQuery()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT * FROM ");
            stringBuilder.Append(_tableConfiguration.Table);
            stringBuilder.Append(" WHERE [Id] = @Id");

            return stringBuilder.ToString();
        }

        private string BuildDeleteStatement()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("DELETE FROM ");
            stringBuilder.Append(_tableConfiguration.Table);
            stringBuilder.Append(" WHERE [Id] = @Id");

            return stringBuilder.ToString();
        }

        private string BuildAddCommand(TDataEntity dataEntity)
        {
            var properties = GetPropertyInfo(dataEntity);
            var columns = string.Join(",", properties.Select(x => x.Name));
            var output = string.Join(",", properties.Select(x => $"INSERTED.{x.Name}"));
            var values = string.Join(",", properties.Select(x => $"@{x.Name}"));

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("INSERT INTO ");
            stringBuilder.Append(_tableConfiguration.Table);
            stringBuilder.Append(" (");
            stringBuilder.Append(columns);
            stringBuilder.Append(") OUTPUT ");
            stringBuilder.Append(output);
            stringBuilder.Append(" VALUES (");
            stringBuilder.Append(values);
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
