#include "Stdafx.h"
#include "GradientMask.h"

const size_t GradientMask::Edge;

GradientMask::GradientMask()
{
	memset(matrix, 0, Edge * Edge);
	FillMatrix();
}


GradientMask::~GradientMask()
{
}

inline int dist(int x, int y, int mid)
{
	int h = (x < mid) ? mid - x : x - mid + 1;
	int v = (y < mid) ? mid - y : y - mid + 1;

	return max(h, v);
}

void GradientMask::FillMatrix()
{
	int mid = Edge >> 1;
	int radius = Edge >> 1;

	for (int row = 0; row < Edge; row++)
	{
		for (int col = 0; col < Edge; col++)
		{
			double dis = 1.0 * dist(col, row, mid) / radius;
			double alpha = dis < 0.6 ? 1.0 : (cos((dis - 0.6) / 0.4 * M_PI) + 1) * 0.5;
			matrix[col + (row << Level)] = static_cast<unsigned char>(min(255, static_cast<int>(alpha * 256)));
		}
	}
}
