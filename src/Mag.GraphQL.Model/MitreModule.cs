using NGraphQL.CodeFirst;

namespace Mag.GraphQL.Model {

  public class MitreModule : GraphQLModule {

    public MitreModule() {
      this.ObjectTypes.Add(typeof(Matrix), typeof(Tactic), typeof(Technique), typeof(Mitigation), typeof(Identity),
                           typeof(Software), typeof(Group), typeof(ExternalReference)
          );
      this.InputTypes.Add(typeof(Paging));
      this.QueryType = typeof(IMitreQuery);

    }

  }
}
