using System;
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
	public static T? GetResourceOrDefault<T>(ulong resourceId, T? defaultValue) where T : Resource {
		try {
			return GetResource<T>(resourceId);
		} catch (Exception) {
			return defaultValue;
		}
	}
}
