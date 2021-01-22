﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Totvs.Olympus.CrossCutting.DefaultContract;
using Totvs.Olympus.CrossCutting.DTOs;
using Totvs.Olympus.CrossCutting.ExternalServices.Enum;
using Totvs.Olympus.Domain.Entities;
using Totvs.Olympus.Domain.ExternalServices;
using Totvs.Olympus.Domain.RepositoryContracts;
using Totvs.Olympus.Domain.Services;

namespace Totvs.Olympus.API.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/import/courses")]
  public class ImportRoutineController
  {
    private readonly ICourseMongoService _courseService;
    private readonly IGetAllCoursesFromAluraService _getAllService;
    private readonly IGetDetailCoursesService _getDetailCoursesService;
    private readonly IMapper _mapper;
    public ImportRoutineController(ICourseMongoService courseService, 
                                   IGetAllCoursesFromAluraService getAllService, 
                                   IGetDetailCoursesService getDetailCoursesService,
                                   IMapper mapper)
    {
      _courseService = courseService;
      _getAllService = getAllService;
      _getDetailCoursesService = getDetailCoursesService;
      _mapper = mapper;
    }


    [HttpPost]
    [MapToApiVersion("1.0")]
    [Route("{clientNames}")]
    public async Task ImportDataToOlympus(EHttpClientNames clientNames)
    {
      var courses = await _getAllService.GetCourses();

      foreach (var item in courses)
      {
        var detailCourse = await _getDetailCoursesService.GetDetailCourseData(item.Slug);
        try
        {
          var courseInputDto = new CourseInputDTO()
          {
            ExternalId = item.Slug,
            Title = detailCourse.Nome,
            FirstClass = detailCourse.Video1AAula,
            LinkExternalCourse = null,
            Score = detailCourse.Nota,
            Instructors = detailCourse.Instrutores.Select(i => new InstructorDTO() { Name = i.Nome, Expertise = new List<string>() })
          };
          await _courseService.CreateCourse(courseInputDto);
        }
        catch (Exception e)
        {

        }
      }

      return;
    }

  }
}
