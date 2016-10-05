#pragma once
#include "BoxBlurFilter.h"

namespace AzureMediaRedactor
{
	namespace Extensions
	{
		namespace Filter
		{
			using namespace System;
			using namespace System::Collections::Generic;
			using namespace System::ComponentModel::Composition;
			using namespace VPP;
			using namespace AzureMediaRedactor::Models;

			[Export("BLUR", IVideoFilter::typeid)]
			public ref class BlurFilterWrapper : public IVideoFilter
			{
				BoxBlurFilter * _filter;
				IVideoFilterProvider ^ _provider;
				int _userData;

			public:
				BlurFilterWrapper();
				~BlurFilterWrapper();
				!BlurFilterWrapper();

				virtual void Filter(IVideoFrame ^ frame);
				virtual void SetProvider(IVideoFilterProvider ^provider, int userData);
			};
		}
	}
}