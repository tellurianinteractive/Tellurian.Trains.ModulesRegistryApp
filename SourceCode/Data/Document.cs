using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Document
    {
        public Document()
        {
            ModuleDxfDrawingNavigations = new HashSet<Module>();
            ModulePdfDocumentationNavigations = new HashSet<Module>();
        }

        public int Id { get; set; }
        public string FileSuffix { get; set; }
        public byte[] Content { get; set; }

        public virtual ICollection<Module> ModuleDxfDrawingNavigations { get; set; }
        public virtual ICollection<Module> ModulePdfDocumentationNavigations { get; set; }
    }
}
