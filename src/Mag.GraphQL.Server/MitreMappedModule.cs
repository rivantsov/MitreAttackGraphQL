using Mag.Data;
using Mag.GraphQL.Model;

namespace Mag.GraphQL.Server {

  /// <summary>Shared Mitre module. Contains resolver registrations and entity mappings. </summary>
  public class MitreMappedModule : MitreModule {

    public MitreMappedModule() : base() {

      this.ResolverClasses.Add(typeof(MitreResolvers));
      // mappings
      MapEntity<IMatrix>().To<Matrix>();
      MapEntity<ITactic>().To<Tactic>();
      MapEntity<IMitigation>().To<Mitigation>();
      MapEntity<ISoftware>().To<Software>(e => new Software() {
        Platforms = e.Platforms.ToStringList(),
        Aliases = e.Aliases.ToStringList()
      });
      MapEntity<IGroup>().To<Group>();
      MapEntity<IExternalRef>().To<ExternalReference>();
      MapEntity<ITechnique>().To<Technique>(
          e => new Technique() {
            Platforms = e.Platforms.ToStringList(),
            Contributors = e.Contributors.ToStringList(),
            PerimissionsRequired = e.PerimissionsRequired.ToStringList(),
            DataSources = e.DataSources.ToStringList(),
            BypassDefenses = e.BypassDefenses.ToStringList()
          }
          );
      MapEntity<IIdentity>().To<Identity>();
    }

  }
}
