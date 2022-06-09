using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vita.Entities;

namespace Mag.Data {

  // basic type for single-string records storing lists like platforms, defences etc
  public interface ILookupTermBase {
    [PrimaryKey(Clustered = true), Auto]
    Guid Id { get; }
    [Size(100)]
    string Value { get; set; }
  }

  [PrimaryKey("Owner,Term", Clustered = true)]
  public interface ILookupTermLink<TOwner, TTerm> {
    [CascadeDelete, Index]
    TOwner Owner { get; set; }
    [CascadeDelete]
    TTerm Term { get; set; }
  }

  public class LookupTermsCache {
    IEntitySession _session;
    Dictionary<Type, List<ILookupTermBase>> _terms = new Dictionary<Type, List<ILookupTermBase>>();

    public LookupTermsCache(IEntitySession session) {
      _session = session;
      LoadTable<ILkpContributor>();
      LoadTable<ILkpDataSource>();
      LoadTable<ILkpDefense>();
      LoadTable<ILkpKillChainPhase>();
      LoadTable<ILkpPlatform>();
      LoadTable<ILkpUserRole>();
    }

    public TEnt GetOrAdd<TEnt>(string value) where TEnt : class, ILookupTermBase {
      var list = GetList<TEnt>();
      var existing = list.FirstOrDefault(t => t.Value == value);
      if (existing != null)
        return (TEnt)existing;
      var ent = _session.NewEntity<TEnt>();
      ent.Value = value;
      list.Add(ent);
      return ent;
    }

    public List<ILookupTermBase> GetList<T>() {
      if (_terms.TryGetValue(typeof(T), out var list))
        return list;
      list = new List<ILookupTermBase>();
      _terms[typeof(T)] = list;
      return list;

    }

    private void LoadTable<TEnt>() where TEnt : class, ILookupTermBase {
      var list = GetList<TEnt>();
      list.AddRange(_session.EntitySet<TEnt>().ToList());
    }
  }

}
