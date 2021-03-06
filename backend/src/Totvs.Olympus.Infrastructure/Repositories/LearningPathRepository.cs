﻿using AutoMapper;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Totvs.Olympus.CrossCutting.DefaultContract;
using Totvs.Olympus.CrossCutting.DTOs;
using Totvs.Olympus.Domain.Entities;
using Totvs.Olympus.Domain.Interfaces;
using Totvs.Olympus.Domain.RepositoryContracts;
using Totvs.Olympus.Infrastructure.Adapter;
using Totvs.Olympus.Infrastructure.Models;

namespace Totvs.Olympus.Infrastructure.Services
{
  public class LearningPathRepository : ILearningPathRepository
  {
    private readonly IMongoCollection<LearningPath> _collection;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notificationContext;

    public LearningPathRepository(IOlympusDatabaseSettings settings,
                                  IMapper mapper,
                                  INotificationContext notificationContext)
    {
      var client = new MongoClient(settings.ConnectionString);
      var database = client.GetDatabase(settings.DatabaseName);

      _collection = database.GetCollection<LearningPath>(nameof(LearningPath));
      _mapper = mapper;
      _notificationContext = notificationContext;
    }

    public async Task<LearningPath> Insert(LearningPath learningPath)
    {
      await _collection.InsertOneAsync(learningPath);
      return learningPath;
    }

    public async Task<LearningPath> Update(Guid Id, LearningPath learningPath)
    {
      var filter = Builders<LearningPath>.Filter.Eq(lp => lp.Id, Id);
      var update = Builders<LearningPath>.Update.Set(lp => lp.Name, learningPath.Name)
                                                .Set(lp => lp.Description, learningPath.Description)
                                                .Set(lp => lp.Courses, learningPath.Courses)
                                                .Set(lp => lp.EmployeeRoles, learningPath.EmployeeRoles)
                                                .Set(lp => lp.DateUpdated, DateTime.Now);

      await _collection.UpdateOneAsync(filter, update);
      var result = await LoadById(Id);

      return result;
    }

    public async Task Delete(Guid Id)
    {
      var result = await _collection.DeleteOneAsync(lp => lp.Id == Id);
      return;
    }

    public async Task<LearningPath> LoadById(Guid Id)
    {
      var ret = await _collection.FindAsync(x => x.Id == Id);
      var result = await ret.FirstOrDefaultAsync();

      if (result is null)
      {
        _notificationContext.AddNotification("NO_LEARNING_PATH_FOUND.", "No Learning Path was found with this Id.", EStatusCodeNotification.NotFound);
        return null;
      }

      return result;
    }

    public IQueryResult<LearningPath> GetAllPaginated(string filter, RequestAllOptionsDTO optionsDTO)
    {
      var result = _collection.AsQueryable();

      if (!string.IsNullOrEmpty(filter))
        result = result.Where(x => x.Name.ToLower().Contains(filter.ToLower()));

      return AutoMapperQueryble.Paginate(result, optionsDTO);
    }
  }
}
