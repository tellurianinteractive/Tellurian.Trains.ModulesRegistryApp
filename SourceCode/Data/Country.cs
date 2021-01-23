using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Country
    {
        public Country()
        {
            Groups = new HashSet<Group>();
            People = new HashSet<Person>();
            Regions = new HashSet<Region>();
        }

        public int Id { get; set; }
        public string EnglishName { get; set; }
        public string DomainSuffix { get; set; }
        public string Languages { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<Person> People { get; set; }
        public virtual ICollection<Region> Regions { get; set; }
    }
}
