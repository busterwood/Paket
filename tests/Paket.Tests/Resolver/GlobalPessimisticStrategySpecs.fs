﻿module Paket.Resolver.GlobalPessimisticStrategySpecs

open Paket
open NUnit.Framework
open FsUnit
open TestHelpers
open Paket.Domain
open Paket.PackageResolver

let graph = [
    "Nancy.Bootstrappers.Windsor","0.23",["Castle.Windsor",VersionRequirement(VersionRange.AtLeast "3.2.1",PreReleaseStatus.No)]
    "Castle.Windsor","3.2.1",[]
    "Castle.Windsor","3.3.0",[]
]

let config1 = """
strategy min
source http://nuget.org/api/v2

nuget Nancy.Bootstrappers.Windsor ~> 0.23
"""

[<Test>]
let ``should resolve simple config1``() = 
    let cfg = DependenciesFile.FromCode(config1)
    let resolved = ResolveWithGraph(cfg,noSha1,VersionsFromGraphAsSeq graph, PackageDetailsFromGraph graph).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()
    getVersion resolved.[PackageName "Castle.Windsor"] |> shouldEqual "3.2.1"
    getVersion resolved.[PackageName "Nancy.Bootstrappers.Windsor"] |> shouldEqual "0.23"

let config2 = """
strategy min
source http://nuget.org/api/v2

nuget Castle.Windsor
nuget Nancy.Bootstrappers.Windsor ~> 0.23
"""

[<Test>]
let ``should resolve simple config2``() = 
    let cfg = DependenciesFile.FromCode(config2)
    let resolved = ResolveWithGraph(cfg,noSha1,VersionsFromGraphAsSeq graph, PackageDetailsFromGraph graph).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()
    getVersion resolved.[PackageName "Castle.Windsor"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Nancy.Bootstrappers.Windsor"] |> shouldEqual "0.23"


let config3 = """
strategy min
source http://nuget.org/api/v2

nuget Nancy.Bootstrappers.Windsor ~> 0.23
nuget Castle.Windsor
"""

[<Test>]
let ``should resolve simple config3``() = 
    let cfg = DependenciesFile.FromCode(config3)
    let resolved = ResolveWithGraph(cfg,noSha1,VersionsFromGraphAsSeq graph, PackageDetailsFromGraph graph).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()
    getVersion resolved.[PackageName "Castle.Windsor"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Nancy.Bootstrappers.Windsor"] |> shouldEqual "0.23"


let graph2 = [
    "Nancy.Bootstrappers.Windsor","0.23",["Castle.Windsor",VersionRequirement(VersionRange.AtLeast "3.2.1",PreReleaseStatus.No)]
    "Castle.Windsor","3.2.0",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.2.0",PreReleaseStatus.No)]
    "Castle.Windsor","3.2.1",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.2.0",PreReleaseStatus.No)]
    "Castle.Windsor","3.3.0",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.3.0",PreReleaseStatus.No)]
    "Castle.Windsor-NLog","3.2.0.1",["Castle.Core-NLog",VersionRequirement(VersionRange.AtLeast "3.2.0",PreReleaseStatus.No)]
    "Castle.Windsor-NLog","3.3.0",["Castle.Core-NLog",VersionRequirement(VersionRange.AtLeast "3.3.0",PreReleaseStatus.No)]
    "Castle.Core-NLog","3.2.0",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.2.0",PreReleaseStatus.No)]
    "Castle.Core-NLog","3.3.0",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.3.0",PreReleaseStatus.No)]
    "Castle.Core-NLog","3.3.1",["Castle.Core",VersionRequirement(VersionRange.AtLeast "3.3.1",PreReleaseStatus.No)]
    "Castle.Core","3.2.0",[]
    "Castle.Core","3.2.1",[]
    "Castle.Core","3.2.2",[]
    "Castle.Core","3.3.0",[]
    "Castle.Core","3.3.1",[]
]

let config4 = """
strategy min
source http://nuget.org/api/v2

nuget Castle.Windsor-NLog
nuget Nancy.Bootstrappers.Windsor ~> 0.23
"""

[<Test>]
let ``should resolve simple config4``() = 
    let cfg = DependenciesFile.FromCode(config4)
    let resolved = ResolveWithGraph(cfg,noSha1,VersionsFromGraphAsSeq graph2, PackageDetailsFromGraph graph2).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()
    getVersion resolved.[PackageName "Castle.Windsor"] |> shouldEqual "3.2.1"
    getVersion resolved.[PackageName "Nancy.Bootstrappers.Windsor"] |> shouldEqual "0.23"

let config5 = """
strategy min
source http://nuget.org/api/v2

nuget Nancy.Bootstrappers.Windsor @~> 0.23
"""

[<Test>]
let ``should override global strategy``() = 
    let cfg = DependenciesFile.FromCode(config5)
    let resolved = ResolveWithGraph(cfg,noSha1,VersionsFromGraphAsSeq graph, PackageDetailsFromGraph graph).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()
    getVersion resolved.[PackageName "Castle.Windsor"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Nancy.Bootstrappers.Windsor"] |> shouldEqual "0.23"

let config6 = """
strategy min
source http://nuget.org/api/v2

nuget Castle.Core-NLog @~> 3.2
nuget Castle.Windsor-NLog !~> 3.2
"""

let resolveWithUpdateMode graph updateMode (cfg : DependenciesFile) =
    let groups = [Constants.MainDependencyGroup, None ] |> Map.ofSeq
    cfg.Resolve(true,noSha1,VersionsFromGraphAsSeq graph,PackageDetailsFromGraph graph,groups,updateMode).[Constants.MainDependencyGroup].ResolvedPackages.GetModelOrFail()

[<Test>]
let ``should update to lastest when updating all``() = 
    let resolved =
        DependenciesFile.FromCode(config6)
        |> resolveWithUpdateMode graph2 UpdateMode.UpdateAll
    getVersion resolved.[PackageName "Castle.Windsor-NLog"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Castle.Core-NLog"] |> shouldEqual "3.3.1"
    getVersion resolved.[PackageName "Castle.Core"] |> shouldEqual "3.3.1"

[<Test>]
let ``should respect overrides when updating single package``() = 
    let resolved =
        DependenciesFile.FromCode(config6)
        |> resolveWithUpdateMode graph2 (UpdateMode.UpdatePackage(Constants.MainDependencyGroup, PackageName "Castle.Windsor-NLog"))
    getVersion resolved.[PackageName "Castle.Windsor-NLog"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Castle.Core-NLog"] |> shouldEqual "3.3.0"
    getVersion resolved.[PackageName "Castle.Core"] |> shouldEqual "3.3.1"
