using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mag.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vita.Entities;

namespace Mag.Import
{
    public class MitreStixFileImporter
    {
        MitreDataEntityApp _app;
        IEntitySession _session; 
        LookupTermsCache _termsCache;
        IList<JObject> _allJObjects;
        Dictionary<string, List<JObject>> _jObjectGroups; 
        HashSet<Guid> _allIds = new HashSet<Guid>();
        Dictionary<string, ITactic> _tacticsByShortName = new Dictionary<string, ITactic>(); 

        public MitreStixFileImporter(MitreDataEntityApp app)
        {
            _app = app; 
        }

        public void ImportStixFile(string path)
        {
            Console.WriteLine($"Loading MITRE ATT&CK file: {path} ...");
            if (!File.Exists(path))
            {
                Console.WriteLine("  File not found, skipping.");
                return; 
            }
            var json = File.ReadAllText(path);
            var rootObj = JsonConvert.DeserializeObject(json);
            var bundle = rootObj as JObject;
            var propObjects = bundle["objects"] as JArray;
            Console.WriteLine($"Found {propObjects.Count} objects in the file. Importing...");
            _allJObjects = propObjects.OfType<JObject>().ToList(); 
            _session = _app.OpenSession();
            // Part 1 - save some raw objects
            LoadAllIds();
            _termsCache = new LookupTermsCache(_session);
            // group
            _jObjectGroups = propObjects.OfType<JObject>()
                .GroupBy(obj => obj["type"].ToString())
                .ToDictionary(g => g.Key, g => g.ToList());

            // Convert/save group by group
            SaveObjects("identity");
            SaveObjects("x-mitre-matrix");
            SaveObjects("x-mitre-tactic");
            AddMatrixTacticsLinks();

            SaveObjects("attack-pattern");

            SaveObjects("malware");
            SaveObjects("tool");
            SaveObjects("intrusion-set");
            SaveObjects("course-of-action");
            _session.SaveChanges();

            ProcessRelationships();
            _session.SaveChanges();

            Console.WriteLine($"Done. Objects imported and saved successfully.");
        }

        public void DeleteAll()
        {
            var session = _app.OpenSession();
            session.DeleteAllIn<ILinkTacticTechnique>();
            session.DeleteAllIn<ILinkTechniqueContributor>();
            session.DeleteAllIn<ILinkTechniqueDataSource>();
            session.DeleteAllIn<ILinkTechniqueDefense>();
            session.DeleteAllIn<ILinkTechniqueKillChainPhase>();
            session.DeleteAllIn<ILinkTechniquePlatform>();
            session.DeleteAllIn<ILinkTechniqueUserRole>();
            session.DeleteAllIn<IRelGroupTechnique>();
            session.DeleteAllIn<IRelMitigationTechnique>();
            session.DeleteAllIn<IRelSoftwareTechnique>();
            session.DeleteAllIn<ITechnique>();


            session.DeleteAllIn<ITactic>();
            session.DeleteAllIn<IMitigation>();
            session.DeleteAllIn<ISoftwareAlias>();
            session.DeleteAllIn<ISoftware>();
            session.DeleteAllIn<IGroup>();
            session.DeleteAllIn<IMatrix>();
            session.DeleteAllIn<IExternalRef>();
            session.DeleteAllIn<IExternalRefList>();
            // delete lookup tables; links (ex: Technique to Platform) is deleted automatically thru CascadeDelete
            session.DeleteAllIn<ILkpContributor>();
            session.DeleteAllIn<ILkpDataSource>();
            session.DeleteAllIn<ILkpDefense>();
            session.DeleteAllIn<ILkpKillChainPhase>();
            session.DeleteAllIn<ILkpPlatform>();
            session.DeleteAllIn<ILkpUserRole>();
        }

        private void AddMatrixTacticsLinks()
        {
            var mxObjs = _jObjectGroups["x-mitre-matrix"];
            foreach(var mxObj in mxObjs)
            {
                var mxId = GetId(mxObj); 
                var tacticIds = MitreDataExtensions.ReadStringList(mxObj, "tactic_refs");
                var index = 0; 
                foreach(var strId in tacticIds)
                {
                    var tacticId = GetId(strId);
                    var linkEnt = _session.NewLinkMatrixTactic(mxId, tacticId, index++);
                }
            }
        }

        private void LoadAllIds()
        {
            _allIds.Clear(); 
            LoadIdsFrom<IIdentity>();
            LoadIdsFrom<IMatrix>();
            LoadIdsFrom<ITactic>();
            LoadIdsFrom<ITechnique>();
            LoadIdsFrom<IGroup>();
            LoadIdsFrom<ISoftware>();
            LoadIdsFrom<IMitigation>();
            LoadIdsFrom<IRelGroupSoftware>();
            LoadIdsFrom<IRelGroupTechnique>();
            LoadIdsFrom<IRelMitigationTechnique>();
            LoadIdsFrom<IRelSoftwareTechnique>();
        }

        private void LoadIdsFrom<TEnt>() where TEnt: class, IStixDomainObject
        {
            var ids = _session.EntitySet<TEnt>().Select(e => e.Id).ToArray();
            _allIds.UnionWith(ids); 
        }

        private void SaveObjects(string groupName)
        {
            if (!_jObjectGroups.TryGetValue(groupName, out var objects))
                return; 
            foreach (var jobj in objects)
            {
                SaveObject(jobj);
                // This is workaround, bug in VITA, it fails to count Db parameters in batch
                // and it goes over 2100 (limit). So we do intermittent saves
                if(_session.GetChangeCount() > 1000)
                    _session.SaveChanges(); 
            }
        }

        private void SaveObject(JObject jobj)
        {
            var type = jobj.Value<string>("type");
            if (type == "x-mitre-collection") //we do not use it; and also it has invalid ID (bad Guid)
                return; 

            var id = GetId(jobj);
            // check if object is already in db
            if (_allIds.Contains(id))
                return;

            switch (type)
            {
                case "x-mitre-matrix":
                    var iMx = _session.NewMatrix(jobj);
                    break;
                case "x-mitre-tactic":
                    var iTc = _session.NewTactic(jobj);
                    _tacticsByShortName[iTc.ShortName] = iTc; // will be used in linking techniques
                    break;
                case "attack-pattern":
                    // TODO: fix moblie Ada type arg
                    var iTech = _session.NewTechnique(jobj) ;
                    ProcessTermList<ILkpContributor, ILinkTechniqueContributor>(jobj, "x_mitre_contributors", iTech);
                    ProcessTermList<ILkpDataSource, ILinkTechniqueDataSource>(jobj, "x_mitre_data_sources", iTech);
                    ProcessTermList<ILkpDefense, ILinkTechniqueDefense>(jobj, "x_mitre_defense_bypassed", iTech);
                    ProcessTermList<ILkpPlatform, ILinkTechniquePlatform>(jobj, "x_mitre_platforms", iTech);
                    ProcessTermList<ILkpUserRole, ILinkTechniqueUserRole>(jobj, "x_mitre_permissions_required", iTech);
                    ProcessTechniqueKillPhaseList(jobj, iTech); 
                    break;
                case "malware":
                case "tool":
                    var iSoft = _session.NewSoftware(jobj, type);
                    ProcessSoftwareAliases(jobj, iSoft); 
                    break;
                case "intrusion-set":
                    var iGrp = _session.NewGroup(jobj);
                    break;
                case "course-of-action":
                    var iMit = _session.NewMitigation(jobj);
                    break;
                case "identity":
                    _session.NewIdentity(jobj);
                    break;
                // 'relationship' is processed in a separate method
            }
            _allIds.Add(id);
        }

        /*
        Techniques map into tactics by use of their kill_chain_phases property. Where the kill_chain_name is mitre-attack, mitre-mobile-attack,
        or mitre-ics-attack (for enterprise, mobile, and ics domains respectively), the phase_name corresponds to the x_mitre_shortname property of an x-mitre-tactic object.          
         */
        private void ProcessTechniqueKillPhaseList(JObject jobj, ITechnique iTech)
        {
            var jPhases = jobj.Value<JArray>("kill_chain_phases");
            if (jPhases == null || jPhases.Count == 0)
                return; 
            foreach(JObject jPh in jPhases)
            {
                var chainName = jPh.Value<string>("kill_chain_name");
                var phaseName = jPh.Value<string>("phase_name");
                if (MitreDataExtensions.IsMitreSourceName(chainName))
                {
                    if (!_tacticsByShortName.TryGetValue(phaseName, out var iTactic))
                        throw new Exception($"Reference to tactic '{phaseName}' in kill_chain_phases is invalid. Technique id: {iTech.Id}");
                    var link = _session.NewEntity<ILinkTacticTechnique>();
                    link.Tactic = iTactic;
                    link.Technique = iTech;
                } else
                {
                    //never happens
                }
            }
        }

        private void ProcessTermList<TLkp, TLnk>(JObject jObj, string propName, IStixDomainObject iParent) 
              where TLkp: class, ILookupTermBase where TLnk: class
        {
            var terms = jObj.ReadStringList(propName);
            if (terms == null || terms.Length == 0)
                return;
            // there are dupes in file there, so use Distinct (dupe ex: attack-pattern--035bb001-ab69-4a0b-9f6c-2de8b09e1b9d, the data source Network Flow repeats twice)
            foreach (var term in terms.Distinct())
            {
                // Get from cache or create record with lookup value 
                var iLkp = _termsCache.GetOrAdd<TLkp>(term); 
                // Link it to parent object
                var iLnk = _session.NewEntity<TLnk>();    
                EntityHelper.SetProperty(iLnk, "Term", iLkp);
                EntityHelper.SetProperty(iLnk, "Owner", iParent);
            }
        }

        private void ProcessSoftwareAliases(JObject jObj, ISoftware software)
        {
            var terms = jObj.ReadStringList("x_mitre_aliases");
            if (terms == null || terms.Length == 0)
                return;
            foreach (var term in terms)
            {
                var iAlias = _session.NewEntity<ISoftwareAlias>();
                iAlias.Alias = term;
                iAlias.Software = software;
            }
        }

        private void ProcessRelationships()
        {
            var relObjects = _jObjectGroups["relationship"];
            // Preload ALL techniques, groups, software into lists, to make sure they stay cached in session, 
            //  so lookup by Id is from memory
            var listTechs = _session.GetEntities<ITechnique>();
            var listSoft = _session.GetEntities<ISoftware>();
            var listMit = _session.GetEntities<IMitigation>();
            var listGroups = _session.GetEntities<IGroup>();

            foreach (var jrel in relObjects)
            {
                if (_session.GetChangeCount() > 1000)
                    _session.SaveChanges();

                var id = GetId(jrel);
                if (ObjectExists(id))
                    continue; //some rels have copies in multiple files
                _allIds.Add(id);
                var refType = jrel.Value<string>("relationship_type");
                var srcRef = jrel.Value<string>("source_ref");
                MitreDataExtensions.ParseId(srcRef, out var srcType, out var srcId);
                var targetRef = jrel.Value<string>("target_ref");
                MitreDataExtensions.ParseId(targetRef, out var targetType, out var targetId);

                var tag = $"{srcType}/{refType}/{targetType}"; //merge all together
                switch (tag)
                {
                    case "intrusion-set/uses/malware":
                    case "intrusion-set/uses/tool":
                        // group -> Software 
                        var soft = _session.GetEntity<ISoftware>(targetId);
                        // there are 2 dangling refs to malware in ent file, filed a bug; until then skip this relationship
                        if (soft == null)
                            break;
                        var lnkGS = _session.NewRelGroupSoftware(jrel, srcId, targetId);
                        break;

                    case "intrusion-set/uses/attack-pattern":
                        // group -> technique 
                        var lnkGT = _session.NewRelGroupTechnique(jrel, srcId, targetId);
                        break;

                    case "malware/uses/attack-pattern":
                    case "tool/uses/attack-pattern":
                        // software -> technique 
                        var lnkST = _session.NewRelSoftwareTechnique(jrel, srcId, targetId);
                        break;

                    case "course-of-action/mitigates/attack-pattern":
                        //  mitigation -> technique 
                        var lnkMT = _session.NewRelMitigationTechnique(jrel, srcId, targetId);
                        break;

                    case "attack-pattern/subtechnique-of/attack-pattern":
                        var child = _session.GetEntity<ITechnique>(srcId);
                        var parent = _session.GetEntity<ITechnique>(targetId);
                        child.IsSubTechniqueOf = parent;
                        break;

                    default:
                        if (refType == "revoked-by")
                        {
                            //TODO: finish this
                        }
                        else
                        {
                            throw new Exception($"Invalid relationship type/args; tag: {tag} ");
                        }
                        break;
                }
            }
        }



        private Guid GetId(JObject jobj, string propName = "id")
        {
            var idStr = jobj.Value<string>(propName);
            var id = GetId(idStr);
            return id; 
        }

        private Guid GetId(string idStr)
        {
            return MitreDataExtensions.GetId(idStr); 
        }

        private bool ObjectExists(JObject jobj)
        {
            var id = GetId(jobj);
            return ObjectExists(id); 
        }

        private bool ObjectExists(Guid id)
        {
            return _allIds.Contains(id);
        }

  }

  static class ImportExtensions {
    internal static void DeleteAllIn<TEnt>(this IEntitySession session) where TEnt : class {
      session.ExecuteDelete<TEnt>(session.EntitySet<TEnt>());
    }

  }
}
