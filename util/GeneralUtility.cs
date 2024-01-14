using System;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Raele.SuperCharacter3D;

public static class GeneralUtility {
	public static T GetResource<T>(ulong resourceId) where T : Resource {
		GodotObject resource = Resource.InstanceFromId(resourceId);
		if (resource == null) {
			throw new Exception($"Resource not found. Resource id: {resourceId}");
		}
		if (!(resource is T resourceT)) {
			throw new Exception($"Failed to retrieve resource. Cause: Unexpected type. Resource id: {resourceId}. Expected type: {typeof(T).Name}. Actual type: {resource.GetType().Name}.");
		}
		return resourceT;
	}
	public static bool GetResource<T>(ulong resourceId, [NotNullWhen(true)] out T? resource) where T : Resource {
		try {
			resource = GetResource<T>(resourceId);
			return true;
		} catch (Exception) {
			resource = null;
			return false;
		}
	}
	public static T? GetResourceOrDefault<T>(ulong resourceId, T? defaultValue = default) where T : Resource {
		GetResource(resourceId, out T? resource);
		return resource ?? defaultValue;
	}

	public static bool FindChild(Node parent, [NotNullWhen(true)] out Node? node, Func<Node, bool> iteratee) {
		foreach(Node child in parent.GetChildren()) {
			if (iteratee(child)) {
				node = child;
				return true;
			}
		}
		node = null;
		return false;
	}

	public static bool FindChildByType<T>(Node parent, [NotNullWhen(true)] out T? nodeT) where T : Node {
		if (FindChild(parent, out Node? node, child => child != null && child.GetType() == typeof(T)) && node is T _nodeT) {
			nodeT = _nodeT;
			return true;
		}
		nodeT = null;
		return false;
	}
}
