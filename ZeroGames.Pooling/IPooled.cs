// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Pooling;

public interface IPooled
{
	void PreGetFromPool();
	void PreReturnToPool();
}


