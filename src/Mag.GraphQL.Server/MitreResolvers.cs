using Mag.Data;
using Mag.GraphQL.Model;
using NGraphQL.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using Vita.Entities;

namespace Mag.GraphQL.Server {
  public class MitreResolvers : IResolverClass {
    MitreDataEntityApp _app;
    IEntitySession _session;

    public void BeginRequest(IRequestContext request) {
      _app = (MitreDataEntityApp)request.App;
      _session = _app.OpenSession(options: EntitySessionOptions.EnableSmartLoad);
    }

    public void EndRequest(IRequestContext request) {
      // with mutations, normally we should save changes here: 
      // if (_session.GetChangeCount() > 0)
      //  _session.SaveChanges();
    }

    [ResolvesField("matrixes")]
    public IList<IMatrix> GetMatrixes(IFieldContext context) {
      var matrixes = _session.EntitySet<IMatrix>().ToList();
      return matrixes;
    }

    [ResolvesField("matrix")]
    public IMatrix GetMatrix(IFieldContext context, string name) {
      var matrix = _session.EntitySet<IMatrix>().FirstOrDefault(m => m.Name == name);
      return matrix;
    }

    [ResolvesField("tactics", targetType: typeof(IMitreQuery))]
    public IList<ITactic> GetTactics(IFieldContext context, Paging paging = null) {
      paging = paging.DefaultIfNull();
      var tactics = _session.EntitySet<ITactic>()
          .OrderBy(paging.OrderBy) // this is not Linq OrderBy, it is from Vita query extensions
          .Skip(paging.Skip).Take(paging.Take)
          .ToList();
      return tactics;
    }

    [ResolvesField("tactic", targetType: typeof(IMitreQuery))]
    public ITactic GetTactic(IFieldContext context, string name) {
      var tactic = _session.EntitySet<ITactic>().FirstOrDefault(t => t.Name == name || t.ShortName == name);
      return tactic;
    }

    [ResolvesField("techniques", targetType: typeof(IMitreQuery))]
    public IList<ITechnique> GetTechniques(IFieldContext context, Paging paging = null) {
      paging = paging.DefaultIfNull();
      var techniques = _session.EntitySet<ITechnique>()
          .OrderBy(paging.OrderBy) // this is not Linq OrderBy, it is from Vita query extensions
          .Skip(paging.Skip).Take(paging.Take).ToList();
      return techniques;
    }

    [ResolvesField("technique", targetType: typeof(IMitreQuery))]
    public ITechnique GetTechnique(IFieldContext context, string name) {
      var technique = _session.EntitySet<ITechnique>().FirstOrDefault(t => t.Name == name || t.MitreId == name);
      return technique;
    }

    [ResolvesField("softwares", targetType: typeof(IMitreQuery))]
    public IList<ISoftware> GetSoftwares(IFieldContext context, Paging paging = null) {
      paging = paging.DefaultIfNull();
      var softwares = _session.EntitySet<ISoftware>()
          .OrderBy(paging.OrderBy) // this is not Linq OrderBy, it is from Vita query extensions
          .Skip(paging.Skip).Take(paging.Take)
          .ToList();
      return softwares;
    }

    [ResolvesField("software", targetType: typeof(IMitreQuery))]
    public ISoftware GetSoftware(IFieldContext context, string name) {
      var software = _session.EntitySet<ISoftware>().FirstOrDefault(t => t.Name == name || t.MitreId == name);
      return software;
    }

    [ResolvesField("groups", targetType: typeof(IMitreQuery))]
    public IList<IGroup> GetGroups(IFieldContext context, Paging paging = null) {
      paging = paging.DefaultIfNull();
      var groups = _session.EntitySet<IGroup>()
          .OrderBy(paging.OrderBy) // this is not Linq OrderBy, it is from Vita query extensions
          .Skip(paging.Skip).Take(paging.Take)
          .ToList();
      return groups;
    }

    [ResolvesField("group", targetType: typeof(IMitreQuery))]
    public IGroup GetGroup(IFieldContext context, string name) {
      var group = _session.EntitySet<IGroup>().FirstOrDefault(t => t.Name == name || t.MitreId == name);
      return group;
    }

    [ResolvesField("mitigations", targetType: typeof(IMitreQuery))]
    public IList<IMitigation> GetMitigations(IFieldContext context, Paging paging = null) {
      paging = paging.DefaultIfNull();
      var mitigations = _session.EntitySet<IMitigation>()
          .OrderBy(paging.OrderBy) // this is not Linq OrderBy, it is from Vita query extensions
          .Skip(paging.Skip).Take(paging.Take)
          .ToList();
      return mitigations;
    }

    [ResolvesField("mitigation", targetType: typeof(IMitreQuery))]
    public IMitigation GetMitigation(IFieldContext context, string name) {
      var Mitigation = _session.EntitySet<IMitigation>().FirstOrDefault(t => t.Name == name || t.MitreId == name);
      return Mitigation;
    }

    // Used by multiple types
    public IList<IExternalRef> GetExternalReferences(IFieldContext context, IStixDomainObject parent) {
      if (parent.ExternalRefs == null)
        return _emptyRefList;
      else
        return parent.ExternalRefs.Refs;
    }
    private static IList<IExternalRef> _emptyRefList = new IExternalRef[] { };

  }
}
