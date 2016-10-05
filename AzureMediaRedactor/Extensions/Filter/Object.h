#pragma once

namespace VPP
{
	class Object
	{
		float _left;
		float _top;
		float _width;
		float _height;

	public:
		Object(float left, float top, float width, float height)
			: _left(left), _top(top), _width(width), _height(height)
		{}
		virtual ~Object() {}

		float getLeft() const { return _left; }
		float getTop() const { return _top; }
		float getWidth() const { return _width; }
		float getHeight() const { return _height; }

		__declspec(property(get = getLeft)) float Left;
		__declspec(property(get = getTop)) float Top;
		__declspec(property(get = getWidth)) float Width;
		__declspec(property(get = getHeight)) float Height;

		void GetCenter(float * x, float * y) const
		{
			* x = _left + _width / 2;
			* y = _top + _height / 2;
		}

		virtual void Scale(float scaleX, float scaleY, float centerX = 0.5f, float centerY = 0.5f)
		{
			auto midX = _left + _width * centerX;
			auto midY = _top + _height * centerY;
			_left = midX - _width * scaleX * 0.5f;
			_top = midY - _height * scaleY * 0.5f;
			_width *= scaleX;
			_height *= scaleY;
		}

		virtual bool operator ==(const Object & obj) const
		{
			return Equals(*this, obj);
		}

		static float OverlapRatio(const Object & obj1, const Object & obj2)
		{
			auto left = max(obj1._left, obj2._left);
			auto right = min(obj1._left + obj1._width, obj2._left + obj2._width);
			auto top = max(obj1._top, obj2._top);
			auto bottom = min(obj1._top + obj1._height, obj2._top + obj2._height);
			auto width = right - left;
			auto height = bottom - top;
			if (width < 0 || height < 0)
			{
				return 0.0f;
			}
			auto area = width * height;
			return 1.0f * area / (obj1._width * obj1._height + obj2._width * obj2._height - area);
		}

		static bool Equals(const Object & obj1, const Object & obj2)
		{
			float epsilon = numeric_limits<float>::epsilon();
			return abs(obj1._left - obj2._left) < epsilon &&
				abs(obj1._top - obj2._top) < epsilon &&
				abs(obj1._width - obj2._width) < epsilon &&
				abs(obj1._height - obj2._height) < epsilon;
		}
	};
}