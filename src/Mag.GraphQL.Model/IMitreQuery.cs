using NGraphQL.CodeFirst;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mag.GraphQL.Model
{
    public interface IMitreQuery
    {
        [GraphQLName("matrixes")]
        IList<Matrix> GetMatrixes();
        [GraphQLName("matrix"), Null]
        Matrix GetMatrix(string name);

        [GraphQLName("tactics")]
        IList<Tactic> GetTactics(Paging paging = null);
        [GraphQLName("tactic"), Null]
        Tactic GetTactic(string name);

        [GraphQLName("techniques")]
        IList<Technique> GetTechniques(Paging paging = null);
        [GraphQLName("technique"), Null]
        Technique GetTechnique(string name);

        [GraphQLName("softwares")]
        IList<Software> GetSoftware(Paging paging = null);
        [GraphQLName("software"), Null]
        Software GetSoftware(string name);

        [GraphQLName("groups")]
        IList<Group> GetGroups(Paging paging = null);
        [GraphQLName("group"), Null]
        Group GetGroup(string name);

        [GraphQLName("mitigations")]
        IList<Mitigation> GetMitigations(Paging paging = null);
        [GraphQLName("mitigation"), Null]
        Mitigation GetMitigation(string name);

    }
}
