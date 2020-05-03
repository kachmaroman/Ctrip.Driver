using System.Collections.Generic;
using Android.Support.V4.App;

namespace Ctrip.Driver.Adapter
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
       public List<Fragment> Fragments { get; set; }
       public List<string> FragmentNames { get; set; }
        
        public ViewPagerAdapter(FragmentManager fragmentManager) : base(fragmentManager)
        {
            Fragments = new List<Fragment>();
            FragmentNames = new List<string>();
        }

        public void AddFragment(Fragment fragment, string name)
        {
            Fragments.Add(fragment);
            FragmentNames.Add(name);
        }
        public override int Count => Fragments.Count;

        public override Fragment GetItem(int position)
        {
            return Fragments[position];
        }
    }
}