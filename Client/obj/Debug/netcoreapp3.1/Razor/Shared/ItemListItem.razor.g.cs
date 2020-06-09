#pragma checksum "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "cc96de9750cdb32c710ace96a93403e1dcde1b3e"
// <auto-generated/>
#pragma warning disable 1591
namespace Client.Shared
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Client;

#line default
#line hidden
#nullable disable
#nullable restore
#line 1 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
using Microsoft.FSharp.Core;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
using Client.Shared;

#line default
#line hidden
#nullable disable
    public partial class ItemListItem : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, "p");
            __builder.AddMarkupContent(1, "\r\n    ");
            __builder.AddMarkupContent(2, @"<style>
        span.overdue { background-color : red; color : white }
        span.today { background-color : blue; color : white }
        span.later { background-color : lightgray }
        span.completeTitle { font-weight:bold; text-decoration:line-through }
        span.incompleteTitle { font-weight:bold }
    </style>
");
#nullable restore
#line 11 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
      
        string titleStyle = Item.Status.IsComplete ? "completeTitle" : "incompleteTitle";
    

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
     if (Item.Status is DomainTypes.Status.Postponed p)
    {
        DateTime now = DateTime.Now;
        int daysUntil = Convert.ToInt32((p.Item - now).TotalDays);
        (string daysUntilText,string styleName) =
            (daysUntil < 0) ? ("Due","overdue") :
            (daysUntil == 0) ? ("Today","today") :
            ($"+{daysUntil} days","later");

#line default
#line hidden
#nullable disable
            __builder.AddContent(3, "        ");
            __builder.OpenElement(4, "span");
            __builder.AddAttribute(5, "class", 
#nullable restore
#line 22 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                      styleName

#line default
#line hidden
#nullable disable
            );
            __builder.AddContent(6, 
#nullable restore
#line 22 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                                  daysUntilText

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(7, "\r\n");
#nullable restore
#line 23 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
    }

#line default
#line hidden
#nullable disable
            __builder.AddContent(8, "    ");
            __builder.OpenElement(9, "span");
            __builder.AddAttribute(10, "class", 
#nullable restore
#line 24 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                  titleStyle

#line default
#line hidden
#nullable disable
            );
            __builder.AddContent(11, 
#nullable restore
#line 24 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                               Item.Title

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line 24 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                                                 if (Item.Quantity!="")
    {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(12, "span");
            __builder.AddContent(13, " * ");
            __builder.AddContent(14, 
#nullable restore
#line 25 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
               Item.Quantity

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line 25 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                                   }

#line default
#line hidden
#nullable disable
#nullable restore
#line 25 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                                     if (Item.Note!="")
    {

#line default
#line hidden
#nullable disable
            __builder.AddMarkupContent(15, "<br>");
            __builder.OpenElement(16, "span");
            __builder.AddContent(17, 
#nullable restore
#line 26 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                  Item.Note

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line 26 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
                                  }

#line default
#line hidden
#nullable disable
            __builder.CloseElement();
        }
        #pragma warning restore 1998
#nullable restore
#line 29 "C:\Users\justi\source\repos\Groceries\Client\Shared\ItemListItem.razor"
       
    [Parameter]
    public DomainTypes.ItemSummary Item { get; set; }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
