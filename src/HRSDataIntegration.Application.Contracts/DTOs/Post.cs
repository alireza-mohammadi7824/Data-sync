﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRSDataIntegration.DTOs
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid MyProperty { get; set; }
        public bool IsActive { get; set; }
    }
}
