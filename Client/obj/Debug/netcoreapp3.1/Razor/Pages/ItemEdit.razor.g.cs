#pragma checksum "C:\Users\justi\source\repos\Groceries\Client\Pages\ItemEdit.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "cfa8b8d5bd210dd985187b23bc84c8c4f6f1533c"
// <auto-generated/>
#pragma warning disable 1591
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
#line 2 "C:\Users\justi\source\repos\Groceries\Client\Pages\ItemEdit.razor"
using Client.Data;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/itemedit")]
    public partial class ItemEdit : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.AddMarkupContent(0, "<h1>Item edit page</h1>\r\n\r\n");
            __builder.OpenComponent<Client.Shared.ItemEditor>(1);
            __builder.AddAttribute(2, "OnSave", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.EventCallback<DomainTypes.ItemEditorModel>>(Microsoft.AspNetCore.Components.EventCallback.Factory.Create<DomainTypes.ItemEditorModel>(this, 
#nullable restore
#line 8 "C:\Users\justi\source\repos\Groceries\Client\Pages\ItemEdit.razor"
                     OnSave

#line default
#line hidden
#nullable disable
            )));
            __builder.AddAttribute(3, "Model", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<DomainTypes.ItemEditorModel>(
#nullable restore
#line 8 "C:\Users\justi\source\repos\Groceries\Client\Pages\ItemEdit.razor"
                                     itemEditorModel

#line default
#line hidden
#nullable disable
            ));
            __builder.CloseComponent();
        }
        #pragma warning restore 1998
#nullable restore
#line 10 "C:\Users\justi\source\repos\Groceries\Client\Pages\ItemEdit.razor"
       

    protected DomainTypes.ItemEditorModel itemEditorModel;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        itemEditorModel = ItemEditorModel.create;
    }

    protected void OnSave(DomainTypes.ItemEditorModel msg)
    {
        var stateMsg = DomainTypes.StateMessage.NewInsertItem(msg);
        ApplicationState.State = global::State.update(stateMsg, ApplicationState.State);
    }

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Components.InjectAttribute] private ApplicationStateService ApplicationState { get; set; }
    }
}
#pragma warning restore 1591
