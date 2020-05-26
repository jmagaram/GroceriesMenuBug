#pragma checksum "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem - Copy.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e7573a333c5584fc26cc205fb7e2fa8a0ed4d8ce"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace Client.Pages
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
#line 9 "C:\Users\justi\source\repos\Groceries\Client\_Imports.razor"
using Client.Shared;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem - Copy.razor"
using static DomainTypes.Duration;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/edititem")]
    public partial class EditItem___Copy : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 102 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem - Copy.razor"
       
    protected ItemEditor.ViewModel model;
    protected EditContext ctx;

    Dictionary<int, Tuple<string, DomainTypes.Duration>> durationChoices;

    public void a()
    {
    }

    protected void OnTitleChange(ChangeEventArgs c)
    {
        model = ItemEditor.updateTitle((string)c.Value, model);
    }

    protected void OnMakeRecurring()
    {
        var newModel = ItemEditor.addRecurrence(model);
        model = newModel;
    }

    protected void OnPostpone()
    {
        var newModel = ItemEditor.postpone(model, DateTime.Now);
        model = newModel;
    }

    protected void RemovePostponement()
    {
        var newModel = ItemEditor.removePostponement(model);
        model = newModel;
    }

    protected void MarkComplete()
    {
        var newModel = ItemEditor.complete(model);
        model = newModel;
    }

    protected void MarkUncomplete()
    {
        var newModel = ItemEditor.incomplete(model);
        model = newModel;
    }

    protected void SelectPostponeDate(ChangeEventArgs c)
    {
        string dt = (string)c.Value;
        DateTime when = DateTime.ParseExact(dt, "yyyy-MM-dd", null);
        model = ItemEditor.postponeUntil(when, model);
    }

    protected void OnRemoveRecurrence() => model = ItemEditor.removeRecurrence(model);

    protected void OnRecurrenceChange(ChangeEventArgs c)
    {
        int keyOfChosenItem = int.Parse((string)c.Value);
        var chosen = durationChoices[keyOfChosenItem].Item2;
        model = ItemEditor.changeRecurrence(chosen, model);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        durationChoices = new Dictionary<int, Tuple<string, DomainTypes.Duration>>();
        durationChoices.Add(1, Tuple.Create("Every 3 days", D3));
        durationChoices.Add(2, Tuple.Create("Weekly", W1));
        durationChoices.Add(3, Tuple.Create("Every 2 weeks", W2));
        durationChoices.Add(4, Tuple.Create("Every 3 weeks", W3));
        durationChoices.Add(5, Tuple.Create("Monthly", M1));
        durationChoices.Add(6, Tuple.Create("Every 2 months", M2));
        durationChoices.Add(7, Tuple.Create("Every 3 months", M3));
        durationChoices.Add(8, Tuple.Create("Every 4 months", M4));
        durationChoices.Add(9, Tuple.Create("Every 6 months", M6));
        durationChoices.Add(10, Tuple.Create("Every 9 months", M9));
        model = ItemEditor.createNew;
    }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
