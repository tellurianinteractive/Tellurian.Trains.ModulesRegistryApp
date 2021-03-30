﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public class Meeting
    {
        public Meeting()
        {
            Layouts = new HashSet<Layout>();
        }

        public int Id { get; set; }
        public int OrganiserGroupId { get; set; }
        public string PlaceName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public bool IsFremo { get; set; }
        public virtual Group OrganiserGroup { get; set; }
        public virtual ICollection<Layout> Layouts { get; set; }

    }
}