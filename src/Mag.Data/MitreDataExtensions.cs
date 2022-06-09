using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Vita.Entities;

namespace Mag.Data
{
    public static class MitreDataExtensions
    {
        public static TEnt NewLookup<TEnt>(this IEntitySession session, string value) where TEnt : class, ILookupTermBase
        {
            var ent = session.NewEntity<TEnt>();
            ent.Value = value;
            return ent;
        }

        public static void AssignProps(this IStixDomainObject ent, JObject jobj)
        {
            ent.Id = GetId(jobj);
            if (ent is INamedStixObject iNamed)
                iNamed.Name = jobj.Value<string>("name");
            ent.Description = jobj.Value<string>("description");
            ent.CreatedOn = jobj.Value<DateTime>("created");
            ent.ModifiedOn = jobj.Value<DateTime>("modified");
            ent.Domains = GetDomains(jobj);
            var session = EntityHelper.GetSession(ent);
            if (TryGetId(jobj, "created_by_ref", out var createdById))
                ent.CreatedBy = session.GetEntity<IIdentity>(createdById, LoadFlags.Stub);
            if (TryGetId(jobj, "x_mitre_modified_by_ref", out var modById))
                ent.ModifiedBy = session.GetEntity<IIdentity>(modById, LoadFlags.Stub);
            // external refs
            ProcessExternalRefs(ent, jobj);
        }

        private static void ProcessExternalRefs(IStixDomainObject entParent, JObject jobj)
        {
            var refsArr = jobj.Value<JArray>("external_references");
            if(refsArr == null || refsArr.Count == 0)
                return; 
            var session = EntityHelper.GetSession(entParent);
            var listEnt = session.NewEntity<IExternalRefList>();
            listEnt.OwnerType = jobj.Value<string>("type");
            entParent.ExternalRefs = listEnt;
            var iHasMitreId = entParent as IHasMitreId; 
            
            foreach(JObject jref in refsArr)
            {
                var refEnt = session.NewEntity<IExternalRef>();
                refEnt.OwnerList = listEnt;
                refEnt.SourceName = jref.Value<string>("source_name");
                refEnt.ExternalId = jref.Value<string>("external_id");
                refEnt.Url = jref.Value<string>("url");
                // Techniques, groups, software have human readable ID like T123; it is stored as ext ref
                if(refEnt.ExternalId != null && iHasMitreId != null && IsMitreSourceName(refEnt.SourceName))
                    iHasMitreId.MitreId = refEnt.ExternalId; 
            }            
        }

        public static bool IsMitreSourceName(string refSource)
        {
            switch(refSource)
            {
                case "mitre-attack":
                case "mitre-mobile-attack":
                case "mitre-ics-attack":
                    return true;
                default: return false; 
            }

        }

        public static IIdentity NewIdentity(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<IIdentity>();
            ent.AssignProps(jobj);
            return ent;
        }

        public static IMatrix NewMatrix(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<IMatrix>();
            ent.AssignProps(jobj);
            return ent; 
        }

        public static ITactic NewTactic(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<ITactic>();
            ent.AssignProps(jobj);
            ent.ShortName = jobj.Value<string>("x_mitre_shortname");
            return ent;
        }

        public static ITechnique NewTechnique(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<ITechnique>();
            ent.AssignProps(jobj);
            ent.Detection = jobj.Value<string>("x_mitre_detection");
            var sysReqs =ReadStringList(jobj, "x_mitre_system_requirements");
            ent.SystemRequirements = sysReqs == null ? null: string.Join(Environment.NewLine, sysReqs);
            // TODO: fix this
            ent.MobileAdaType = null;
            return ent;
        }

        public static IGroup NewGroup(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<IGroup>();
            ent.AssignProps(jobj);
            return ent;
        }

        public static ISoftware NewSoftware(this IEntitySession session, JObject jobj, string stixType)
        {
            var ent = session.NewEntity<ISoftware>();
            ent.AssignProps(jobj);
            ent.StixType = stixType;
            return ent;
        }

        public static IMitigation NewMitigation(this IEntitySession session, JObject jobj)
        {
            var ent = session.NewEntity<IMitigation>();
            ent.AssignProps(jobj);
            return ent;
        }

        public static IRelGroupSoftware NewRelGroupSoftware(this IEntitySession session, JObject jobj, Guid groupId, Guid softwareId)
        {
            var ent = session.NewEntity<IRelGroupSoftware>();
            ent.AssignProps(jobj);
            ent.Group = session.GetEntity<IGroup>(groupId);
            ent.Software = session.GetEntity<ISoftware>(softwareId);
            return ent;
        }

        public static IRelGroupTechnique NewRelGroupTechnique(this IEntitySession session, JObject jobj, Guid groupId, Guid techniqueId)
        {
            var ent = session.NewEntity<IRelGroupTechnique>();
            ent.AssignProps(jobj);
            ent.Group = session.GetEntity<IGroup>(groupId);
            ent.Technique = session.GetEntity<ITechnique>(techniqueId);
            return ent;
        }

        public static IRelSoftwareTechnique NewRelSoftwareTechnique(this IEntitySession session, JObject jobj, Guid softwareId, Guid techniqueId)
        {
            var ent = session.NewEntity<IRelSoftwareTechnique>();
            ent.AssignProps(jobj);
            ent.Software = session.GetEntity<ISoftware>(softwareId);
            ent.Technique = session.GetEntity<ITechnique>(techniqueId);
            return ent;
        }

        public static IRelMitigationTechnique NewRelMitigationTechnique(this IEntitySession session, JObject jobj, Guid mitigationId, Guid techniqueId)
        {
            var ent = session.NewEntity<IRelMitigationTechnique>();
            ent.AssignProps(jobj);
            ent.Mitigation = session.GetEntity<IMitigation>(mitigationId);
            ent.Technique = session.GetEntity<ITechnique>(techniqueId);
            return ent;
        }



        // ============================================== utilities ==========================================================================
        public static bool TryGetId(JObject jobj, string propName, out Guid id)
        {
            id = Guid.Empty;
            var idStr = jobj.Value<string>(propName);
            if (string.IsNullOrEmpty(idStr))
                return false;
            id = GetId(idStr);
            return true; 
        }

        public static Guid GetId(JObject jobj, string propName = "id")
        {
            var idStr = jobj.Value<string>(propName);
            var id = GetId(idStr);
            return id;
        }

        public static Guid GetId(string idStr)
        {
            try
            {
                var p = idStr.IndexOf("--");
                var guidStr = idStr.Substring(p + 2);
                return Guid.Parse(guidStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse ID: '{idStr}', error: {ex.Message}");
            }
        }

        public static void ParseId(string idStr, out string type, out Guid id)
        {
            var p = idStr.IndexOf("--");
            type = idStr.Substring(0, p);
            var guidStr = idStr.Substring(p + 2);
            id = Guid.Parse(guidStr);
        }



        private static MitreDomains GetDomains(JObject jobj)
        {
            var values = ReadStringList(jobj, "x_mitre_domains");
            if (values == null || values.Length == 0)
                return MitreDomains.None;
            return GetDomains(values);
        }

        public static MitreDomains GetDomains(string[] values)
        {
            var d = MitreDomains.None;
            foreach (var v in values)
                d |= GetDomain(v);
            return d;
        }

        public static MitreDomains GetDomain(string s)
        {
            switch (s)
            {
                case "enterprise-attack": return MitreDomains.Enterprise;
                case "mobile-attack": return MitreDomains.Mobile;
                case "ics-attack": return MitreDomains.Ics;
                default: return MitreDomains.None;
            }
        }

        public static string[] ReadStringList(this JObject jobj, string propName)
        {
            var jArr = jobj.Value<JArray>(propName);
            if (jArr == null)
                return null;
            var strArr = jArr.Select(jt => jt.ToString()).ToArray();
            return strArr;

        }

        public static ILinkMatrixTactic NewLinkMatrixTactic(this IEntitySession session, Guid matrixId, Guid tacticId, int index)
        {
            var ent = session.NewEntity<ILinkMatrixTactic>();
            ent.Matrix = session.GetEntity<IMatrix>(matrixId, LoadFlags.Stub);
            ent.Tactic = session.GetEntity<ITactic>(tacticId, LoadFlags.Stub);
            ent.OrderIndex = index;
            return ent;
        }

        public static ISoftwareAlias AddAlias(this ISoftware software, string alias)
        {
            var session = EntityHelper.GetSession(software);
            var ent = session.NewEntity<ISoftwareAlias>();
            ent.Software = software;
            ent.Alias = alias;
            return ent;
        }

        public static IList<string> ToStringList<TTerm>(this IList<TTerm> terms) where TTerm: ILookupTermBase
        {
            return terms.Select(t => t.Value).ToList(); 
        }

        public static IList<string> ToStringList(this IList<ISoftwareAlias> aliasList)
        {
            return aliasList.Select(t => t.Alias).ToList();
        }

    }
}
