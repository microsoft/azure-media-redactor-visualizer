#pragma once

class GradientMask
{
public:
	GradientMask();
	~GradientMask();

	inline unsigned char Get(int row, int col) const
	{
		return matrix[(row << Level) + col];
	}

	static const size_t Level = 10;

private:
	static const size_t Edge = 1 << Level;
	void FillMatrix();
	unsigned char matrix[Edge * Edge];
};

