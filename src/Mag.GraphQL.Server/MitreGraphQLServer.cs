using Mag.Data;
using NGraphQL.Server;

namespace Mag.GraphQL.Server {

  public class MitreGraphQLServer : GraphQLServer {
    public MitreGraphQLServer(MitreDataEntityApp app, GraphQLServerSettings settings) : base(app, settings) {
      var mod = new MitreMappedModule();
      base.RegisterModules(mod);
    }
  }
}
