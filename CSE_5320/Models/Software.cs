﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CSE_5320.Models
{
    public class Software : Base
    {
        public string SerialNumber { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Os")]
        public int OsId { get; set; }

        [ForeignKey("Memory")]
        public int MemoryId { get; set; }

        [ForeignKey("Cpu")]
        public int CpuId { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }

        [ForeignKey("Technical")]
        public int? TechnicalContact { get; set; }

        public DateTime? ExpiryDate { get; set; }        

        public virtual Category Category { get; set; }

        public virtual User Assigned { get; set; }

        public virtual User Technical { get; set; }

        public virtual Status Status { get; set; }

        public virtual Os Os { get; set; }

        public virtual Cpu Cpu { get; set; }

        public virtual Memory Memory { get; set; }
    }
}