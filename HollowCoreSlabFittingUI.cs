using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Plugins;

namespace HollowCoreSlabFitting
{
    class HollowCoreSlabFittingUI
    {
            public const string HollowCoreSlabFitting = @"" +
            @"page(""TeklaStructures"","""")" + "\n" +
            @"" + "\n" +
            @"{" + "\n" +
            @"    " + "\n" +
            @"  joint(1, HollowCoreSlabFitting Fitting)" + "\n" +
            @"    " + "\n" +
            @"  {" + "\n" +
            @"       " + "\n" +
            @"       " + "\n" +
            @"     tab_page(""HollowCoreSlabFitting Fitting"", j_Parameters, 1)" + "\n" +
            @"        " + "\n" +
            @"     {" + "\n" +
            @"            " + "\n" +
            @"       parameter("""", ""P1"", distance, number, 220, 95)" + "\n" +
            @" " + "\n" +
            @"       picture(""pic_db_fitting.bmp"",,,0,100)  " + "\n" +
            @"     }" + "\n" +
            @"        " + "\n" +
            @"  depend(1)" + "\n" +
            @"   " + "\n" +
            @"  }" + "\n" +
            @"" + "\n" +
            @"}" + "\n";
        }
}
