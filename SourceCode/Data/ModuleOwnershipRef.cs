﻿using ModulesRegistry.Data.Extensions;
using Rationals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data;

public class ModuleOwnershipRef : IEquatable<ModuleOwnershipRef>
{
    private ModuleOwnershipRef() { }
    public static ModuleOwnershipRef Person(int personId) => new() { _PersonId = personId };
    public static ModuleOwnershipRef Person(ClaimsPrincipal? principal, int personId) => new() { _PersonId = personId > 0 ? personId : principal is not null ? principal.PersonId() : 0 };
    public static ModuleOwnershipRef Group(int groupId) => new() { _GroupId = groupId };
    public static ModuleOwnershipRef PersonInGroup(int personId, int groupId) => new() { _PersonId = personId, _GroupId = groupId };
    public static ModuleOwnershipRef Any(ClaimsPrincipal? principal, int personId, int groupId) => new() { _Principal = principal, _PersonId = personId, _GroupId = groupId };
    public static ModuleOwnershipRef PersonAndOrGroup(int personId, int groupId) =>
        personId > 0 && groupId > 0 ? PersonInGroup(personId, groupId) :
        groupId > 0 ? Group(groupId) :
        personId > 0 ? Person(personId) :
        None;
    public static ModuleOwnershipRef PersonAndOrGroup(ClaimsPrincipal? principal, int personId, int groupId) =>
        personId > 0 && groupId > 0 ? PersonInGroup(personId, groupId) :
        groupId > 0 ? Group(groupId) :
        personId > 0 ? Person(personId) :
        principal.AsModuleOwnershipRef();

    public static ModuleOwnershipRef None => new();
    public static ModuleOwnershipRef WithPrincipal(ModuleOwnershipRef original, ClaimsPrincipal? principal) => new()
    {
        _PersonId = original._PersonId,
        _GroupId = original._GroupId,
        _Principal = principal
    };

    public override bool Equals(object? obj) => obj is ModuleOwnershipRef other && Equals(other);
    public bool Equals(ModuleOwnershipRef? other) => other is not null && other.PersonId == PersonId && other.GroupId == GroupId;
    public override int GetHashCode() => PersonId.GetHashCode() ^ GroupId.GetHashCode();
    public static bool operator ==(ModuleOwnershipRef left, ModuleOwnershipRef right) => left.Equals(right);
    public static bool operator !=(ModuleOwnershipRef left, ModuleOwnershipRef right) => !left.Equals(right);


    public int PersonId => _PersonId > 0 ? _PersonId : _GroupId == 0 && _Principal is not null ? _Principal.PersonId() : 0;
    public int GroupId => _GroupId;

    private ClaimsPrincipal? _Principal;
    private int _PersonId;
    private int _GroupId;


    public bool IsOwner(ModuleOwnership ownership) =>
        ownership.OwnedShare > 0 &&
        IsGroup ? ownership.GroupId == GroupId :
        IsPerson && ownership.PersonId == PersonId;

    public bool IsOwner(int id) => PersonId > 0 && id == PersonId || GroupId > 0 && id == GroupId;
    public bool IsGroup => GroupId > 0 && PersonId == 0;
    public bool IsPerson => PersonId > 0 && GroupId == 0;
    public bool IsPersonInGroup => PersonId > 0 && GroupId > 0;
    public bool IsAny => IsPerson || IsGroup || IsPersonInGroup;
    public bool IsNone => PersonId == 0 && GroupId == 0;
    public override string ToString() => $"Person={PersonId},Group={GroupId}";
}

public static class ModuleOwnershipRefExtensions
{
    public static ModuleOwnership ToOwnership(this ModuleOwnershipRef ownership, int moduleId, Rational part) =>
        new()
        {
            PersonId = ownership.PersonId,
            GroupId = ownership.GroupId,
            ModuleId = moduleId,
            OwnedShare = (double)part,
        };

}

