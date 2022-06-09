using NGraphQL.CodeFirst;
using System;
using System.Collections.Generic;

namespace Mag.GraphQL.Model
{
    public class StixObjectBase
    {
        public Guid Id;

        [Null] public string Description;

        [Resolver("GetExternalReferences")]
        public List<ExternalReference> ExternalReferences;

        public DateTime CreatedOn;
        [Null] public Identity CreatedBy;
        public DateTime ModifiedOn;
        [Null] public Identity ModifiedBy;
        
        public Guid? RevokedById { get; set; }

    }

    public class NamedObject : StixObjectBase
    {
        public string Name; 
    }

    public class NamedObjectExt: NamedObject
    {
        [Null] public string MitreId;
    }

    public class Matrix: NamedObject
    {
        public IList<Tactic> Tactics;
    }

  /// <summary>Hello GraphQL@MS! 
  /// Tactics represent the "why" of an ATT&amp;CK technique or sub-technique. </summary>
  public class Tactic : NamedObject
    {
        /// <summary>Techniques used for the tactic. </summary>
        public IList<Technique> Techniques;
        public IList<Matrix> Matrices;
    }

    public class Technique : NamedObjectExt
    {
        public IList<Tactic> Tactics;
        [Null] public string Detection;
        [Null] public string SystemRequirements;
        public IList<Software> UsedBySoftwares;
        public IList<Group> UsedByGroups;
        public IList<Mitigation> Mitigations;

        public IList<string> Platforms;
        public IList<string> Contributors;

        public IList<string> PerimissionsRequired;
        public IList<string> DataSources;
        public IList<string> BypassDefenses;
    }

    public class Software : NamedObjectExt
    {
        public string StixType; //tool or malware
        public IList<string> Platforms;
        public IList<string> Aliases;
        public IList<Technique> UsesTechniques; 
    }

    public class Group : NamedObjectExt
    {
        public IList<Software> UsesSoftwares;
        public IList<Technique> UsesTechniques;
    }

    public class Mitigation : NamedObjectExt
    {
        public IList<Technique> Techniques;
    }

    public class Identity: NamedObject
    {
    }

    public class ExternalReference
    {
        public string SourceName { get; set; }
        [Null] public string ExternalId { get; set; }
        [Null] public string Url { get; set; }
    }

    /// <summary>Holds sorting/paging parameters for lists querying. </summary>
    public class Paging
    {
        /// <summary>Sort order. List of fields to sort by, with optional -desc suffix. For ex: &quot;name,createdOn-desc&quot;. </summary>
        [Null] public string OrderBy;
        public int Skip = 0;
        public int Take = 20;
    }

}
