// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Pooling;

public readonly struct ObjectPoolConfig
{
	public int32 PrecacheCount { get; init; }
	public int32 MaxAliveCount { get; init; }
}


