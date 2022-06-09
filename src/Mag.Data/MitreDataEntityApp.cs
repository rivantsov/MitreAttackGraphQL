using System;
using Vita.Entities;
using Mag.Data;

namespace Mag.Data
{
  public class MitreDataEntityApp : EntityApp
  {
    public MitreEntityModule MainModule; 

    public MitreDataEntityApp() : base(nameof(MitreDataEntityApp))
    {
      // Register schema/area 'ma'; register all your entity modules
      var maSchema = AddArea("ma");
      MainModule = new MitreEntityModule(maSchema);
    }
  }
}
