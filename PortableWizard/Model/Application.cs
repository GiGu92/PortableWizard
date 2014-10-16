using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard.Model
{
	class Application
	{
		public string Name { get; set; }
		public bool IsDesktopShortcut { get; set; }
		public bool IsStartMenuShortcut { get; set; }
		public bool IsPinnedToStart { get; set; }
		public bool IsPinnedToTaskbar { get; set; }
		public bool IsStartup { get; set; }
		public List<string> SupportedFileExtensions { get; set; }
		public List<string> HandledFileExtensions { get; set; }

		public Application() {	}
		
	}
}
