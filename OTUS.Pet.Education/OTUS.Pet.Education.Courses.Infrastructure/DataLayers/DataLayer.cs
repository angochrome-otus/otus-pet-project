using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;

namespace OTUS.Pet.Education.Courses.Infrastructure.DataLayers
{
    public class DataLayer : IDataLayer
    {
        private readonly ICourseLayer _courseLayer;
        private readonly ILessonLayer _lessonLayer;
        private readonly IRoleLayer _roleLayer;
        private readonly ISubjectLayer _subjectLayer;
        private readonly IUserLayer _userLayer;

        /// <inheritdoc/>
        public ICourseLayer CourseLayer => _courseLayer;
        /// <inheritdoc/>
        public ILessonLayer LessonLayer => _lessonLayer;
        /// <inheritdoc/>
        public IRoleLayer RoleLayer => _roleLayer;
        /// <inheritdoc/>
        public ISubjectLayer SubjectLayer => _subjectLayer;
        /// <inheritdoc/>
        public IUserLayer UserLayer => _userLayer;

        public DataLayer(ICourseLayer courseLayer, ILessonLayer lessonLayer, IRoleLayer roleLayer, ISubjectLayer subjectLayer, IUserLayer userLayer)
        {
            _courseLayer = courseLayer;
            _lessonLayer = lessonLayer;
            _roleLayer = roleLayer;
            _subjectLayer = subjectLayer;
            _userLayer = userLayer;
        }
    }
}