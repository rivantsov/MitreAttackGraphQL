using System;
using System.Collections.Generic;
using Vita.Entities;

namespace Mag.Data
{
    [Flags]
    public enum MitreDomains
    {
        None = 0,
        Enterprise = 1,
        Mobile = 1 << 1,
        Ics = 1 << 2
    }

    /// <summary>Mobile Adversary Device Access Type. </summary>
    public enum MobileAdaType
    {
        WithoutAccess = 0,
        PreAccess,
        PostAccess,
    }

    [ClusteredIndex("CreatedOn,Id")]
    public interface IStixDomainObject
    {
        [PrimaryKey]
        Guid Id { get; set; }

        [Unlimited, Nullable]
        string Description { get; set; }
        MitreDomains Domains { get; set; }

        [Nullable]
        IIdentity CreatedBy { get; set; }
        [Nullable]
        IIdentity ModifiedBy { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime ModifiedOn { get; set; }

        [Nullable]
        IExternalRefList ExternalRefs { get; set; }

        Guid? RevokedById { get; set; }
    }

    // separately defining Name; most Stix objects have name except Relations
    public interface INamedStixObject: IStixDomainObject
    {
        [Size(100), Index]
        string Name { get; set; }
    }

    public interface IHasMitreId
    {
        [Size(50), Nullable, Index]
        string MitreId { get; set; }

    }

    [Entity]
    public interface IMatrix : INamedStixObject
    {
        [ManyToMany(typeof(ILinkMatrixTactic))]
        IList<ITactic> Tactics { get; }
    }

    [Entity]
    public interface ITactic : INamedStixObject
    {
        [Size(100)]
        string ShortName { get; set; }

        [ManyToMany(typeof(ILinkTacticTechnique))]
        IList<ITechnique> Techniques { get; }

        [ManyToMany(typeof(ILinkMatrixTactic))]
        IList<IMatrix> Matrices { get; }
    }

    [Entity, PrimaryKey("Matrix,Tactic", Clustered = true), OrderBy("OrderIndex")]
    public interface ILinkMatrixTactic
    {
        IMatrix Matrix { get; set; }
        [CascadeDelete]
        ITactic Tactic { get; set; }
        int OrderIndex { get; set; }
    }


    [Entity]
    public interface ITechnique : INamedStixObject, IHasMitreId
    {
        [ManyToMany(typeof(ILinkTacticTechnique))]
        IList<ITactic> Tactics { get; }

        [Nullable]
        ITechnique IsSubTechniqueOf { get; set; }

        [Unlimited, Nullable]
        string Detection { get; set; }

        [Unlimited, Nullable]
        string SystemRequirements { get; set; }

        MobileAdaType? MobileAdaType { get; set; }

        [ManyToMany(typeof(ILinkTechniqueUserRole))]
        IList<ILkpUserRole> PerimissionsRequired { get; }

        [ManyToMany(typeof(ILinkTechniqueContributor))]
        IList<ILkpContributor> Contributors { get; }

        [ManyToMany(typeof(ILinkTechniqueDataSource))]
        IList<ILkpDataSource> DataSources { get; }

        [ManyToMany(typeof(ILinkTechniquePlatform))]
        IList<ILkpPlatform> Platforms { get; }

        [ManyToMany(typeof(ILinkTechniqueDefense))]
        IList<ILkpDefense> BypassDefenses { get; }

        [ManyToMany(typeof(IRelSoftwareTechnique))]
        IList<ISoftware> UsedBySoftwares { get; }

        [ManyToMany(typeof(IRelGroupTechnique))]
        IList<IGroup> UsedByGroups { get; }

        [ManyToMany(typeof(IRelMitigationTechnique))]
        IList<IMitigation> Mitigations { get; }

    }

    [Entity, PrimaryKey("Tactic,Technique", Clustered = true)]
    public interface ILinkTacticTechnique
    {
        [CascadeDelete, Index]
        ITactic Tactic { get; set; }
        [CascadeDelete]
        ITechnique Technique { get; set; }
    }


    // UserRole ------------------------------------------------------------------------
    [Entity]
    public interface ILkpUserRole : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniqueUserRole : ILookupTermLink<ITechnique, ILkpUserRole> { }

    // Contributor ------------------------------------------------------------------------
    [Entity]
    public interface ILkpContributor : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniqueContributor : ILookupTermLink<ITechnique, ILkpContributor> { }

    // Kill chain ------------------------------------------------------------------------
    [Entity]
    public interface ILkpKillChainPhase : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniqueKillChainPhase : ILookupTermLink<ITechnique, ILkpKillChainPhase> { }

    // Data source ------------------------------------------------------------------------
    [Entity]
    public interface ILkpDataSource : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniqueDataSource : ILookupTermLink<ITechnique, ILkpDataSource> { }

    // Platform ------------------------------------------------------------------------
    [Entity]
    public interface ILkpPlatform : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniquePlatform : ILookupTermLink<ITechnique, ILkpPlatform> { }
    [Entity]
    public interface ILinkSoftwarePlatform : ILookupTermLink<ISoftware, ILkpPlatform> { }

    // Defences ------------------------------------------------------------------------
    [Entity]
    public interface ILkpDefense : ILookupTermBase { }
    [Entity]
    public interface ILinkTechniqueDefense : ILookupTermLink<ITechnique, ILkpDefense> { }

    [Entity] // course-of-action in STIX
    public interface IMitigation : INamedStixObject, IHasMitreId
    {
        [ManyToMany(typeof(IRelMitigationTechnique))]
        IList<ITechnique> Techniques { get; }
    }

    [Entity] // intrusion-set in STIX
    public interface IGroup : INamedStixObject, IHasMitreId
    {
        [ManyToMany(typeof(IRelGroupSoftware))]
        IList<ISoftware> UsesSoftwares { get; }
        [ManyToMany(typeof(IRelGroupTechnique))]
        IList<ITechnique> UsesTechniques { get; }
    }

    [Entity] // malware or tool in STIX
    public interface ISoftware : INamedStixObject, IHasMitreId
    {
        [Size(100)]
        string StixType { get; set; }

        [ManyToMany(typeof(ILinkSoftwarePlatform))]
        IList<ILkpPlatform> Platforms { get; }
        IList<ISoftwareAlias> Aliases { get; }

        [ManyToMany(typeof(IRelSoftwareTechnique))]
        IList<ITechnique> UsesTechniques { get; }
    }

    [Entity, ClusteredIndex("Software,Id")]
    public interface ISoftwareAlias
    {
        [PrimaryKey, Auto]
        Guid Id { get; }
        ISoftware Software { get; set; }
        [Size(100)]
        string Alias { get; set; }
    }

    [Entity]
    public interface IIdentity : INamedStixObject
    {
    }

    [Entity, ClusteredIndex("OwnerList,Id")]
    public interface IExternalRef
    {
        [PrimaryKey, Auto]
        Guid Id { get; }

        [CascadeDelete, Index]
        IExternalRefList OwnerList { get; set; } //owner list

        [Size(100)]
        string SourceName { get; set; }
        [Size(100), Nullable]
        string ExternalId { get; set; }
        [Unlimited, Nullable]
        string Url { get; set; }
    }

    [Entity]
    public interface IExternalRefList
    {
        [PrimaryKey(Clustered = true), Auto]
        Guid Id { get; }

        [Size(100)]
        string OwnerType { get; set; }

        IList<IExternalRef> Refs { get; }
    }

    public interface IRelationBase: IStixDomainObject {  } 

    [Entity]
    public interface IRelGroupSoftware : IRelationBase
    {
        [CascadeDelete, Index]
        IGroup Group { get; set; }
        [CascadeDelete, Index]
        ISoftware Software { get; set; }
    }

    [Entity]
    public interface IRelGroupTechnique : IRelationBase
    {
        [CascadeDelete, Index]
        IGroup Group { get; set; }
        [CascadeDelete, Index]
        ITechnique Technique { get; set; }
    }

    [Entity]
    public interface IRelSoftwareTechnique : IRelationBase
    {
        [CascadeDelete, Index]
        ISoftware Software { get; set; }
        [CascadeDelete, Index]
        ITechnique Technique { get; set; }
    }

    [Entity]
    public interface IRelMitigationTechnique : IRelationBase
    {
        [CascadeDelete, Index]
        IMitigation Mitigation { get; set; }
        [CascadeDelete, Index]
        ITechnique Technique { get; set; }
    }


}
