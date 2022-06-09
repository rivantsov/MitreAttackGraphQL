using System;
using System.Collections.Generic;
using Vita.Entities;

namespace Mag.Data {
  public class MitreEntityModule : EntityModule {
    public static readonly Version CurrentVersion = new Version("1.0.0.0");

    public MitreEntityModule(EntityArea area) : base(area, nameof(MitreEntityModule)) {
      // Register all entity types
      RegisterEntities(
          typeof(IMatrix), typeof(ITactic), typeof(ILinkMatrixTactic), typeof(ITechnique), typeof(ILinkTacticTechnique),
          typeof(IGroup), typeof(IMitigation), typeof(IMitigation), typeof(ISoftware), typeof(ISoftwareAlias),
          typeof(IIdentity), typeof(IExternalRef), typeof(IExternalRefList),
          typeof(ILkpUserRole), typeof(ILkpContributor), typeof(ILkpKillChainPhase), typeof(ILkpDataSource), typeof(ILkpPlatform), typeof(ILkpDefense),
          typeof(ILinkTechniqueContributor), typeof(ILinkTechniqueDataSource), typeof(ILinkTechniqueDefense), typeof(ILinkTechniqueKillChainPhase),
          typeof(ILinkTechniquePlatform), typeof(ILinkTechniqueUserRole), typeof(ILinkSoftwarePlatform),
          typeof(IRelGroupSoftware), typeof(IRelGroupTechnique), typeof(IRelSoftwareTechnique), typeof(IRelMitigationTechnique)
          );
    } //constructor

  }
}
