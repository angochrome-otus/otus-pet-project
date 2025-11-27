using OTUS.Pet.Education.Courses.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Entities;

/// <summary>
/// Описание курса
/// </summary>
public class Subject : Entity
{
  public string Name { get; set; } = string.Empty;
}
