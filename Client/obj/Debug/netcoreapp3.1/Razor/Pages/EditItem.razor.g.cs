#pragma checksum "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5bed3a4e3703f119b23e6ccc8adbfb53ef6985c8"
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
#line 2 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
using static DomainTypes.Duration;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
using static ItemBuilder;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
using Unit = Microsoft.FSharp.Core.Unit;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/edititem")]
    public partial class EditItem : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.AddMarkupContent(0, "<style>\r\n    .validationError {\r\n        color: red;\r\n    }\r\n\r\n    .completedTitle {\r\n        text-decoration: line-through;\r\n        font-weight: bold;\r\n    }\r\n</style>\r\n");
            __builder.AddMarkupContent(1, "<h3>Add/Edit item</h3>\r\n");
            __builder.OpenElement(2, "form");
            __builder.AddMarkupContent(3, "\r\n    ");
            __builder.OpenElement(4, "div");
            __builder.AddMarkupContent(5, "\r\n");
#nullable restore
#line 19 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
         if (model.Schedule.IsComplete)
        {

#line default
#line hidden
#nullable disable
            __builder.AddContent(6, "            ");
            __builder.OpenElement(7, "p");
            __builder.AddAttribute(8, "class", "completedTitle");
            __builder.AddContent(9, 
#nullable restore
#line 21 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                       model.Title

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(10, "\r\n");
#nullable restore
#line 22 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
             if (activate(model).IsSome())
            {

#line default
#line hidden
#nullable disable
            __builder.AddContent(11, "                ");
            __builder.OpenElement(12, "button");
            __builder.AddAttribute(13, "type", "button");
            __builder.AddAttribute(14, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 24 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                Activate

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(15, "Add to shopping list again");
            __builder.CloseElement();
            __builder.AddMarkupContent(16, "\r\n");
#nullable restore
#line 25 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
            }

#line default
#line hidden
#nullable disable
            __builder.AddMarkupContent(17, "            <br>\r\n");
#nullable restore
#line 27 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
        }
        else
        {

#line default
#line hidden
#nullable disable
            __builder.AddContent(18, "            ");
            __builder.OpenElement(19, "label");
            __builder.AddMarkupContent(20, "\r\n                Name: ");
            __builder.OpenElement(21, "input");
            __builder.AddAttribute(22, "oninput", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, 
#nullable restore
#line 31 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                       OnTitleChange

#line default
#line hidden
#nullable disable
            ));
            __builder.AddAttribute(23, "placeholder", "Apples, Canned Tuna, ...");
            __builder.AddAttribute(24, "value", 
#nullable restore
#line 31 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                                     model.Title.Value

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line 31 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                                                           if (model.Title.Error.IsSome())
                {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(25, "span");
            __builder.AddAttribute(26, "class", "validationError");
            __builder.AddContent(27, 
#nullable restore
#line 32 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                model.Title.Error.Value

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line 32 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                              }

#line default
#line hidden
#nullable disable
            __builder.AddContent(28, "            ");
            __builder.CloseElement();
            __builder.AddMarkupContent(29, "\r\n");
#nullable restore
#line 34 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
        }

#line default
#line hidden
#nullable disable
            __builder.AddContent(30, "    ");
            __builder.CloseElement();
            __builder.AddMarkupContent(31, "\r\n    ");
            __builder.OpenElement(32, "div");
            __builder.AddMarkupContent(33, "\r\n        ");
            __builder.OpenElement(34, "label");
            __builder.AddMarkupContent(35, "\r\n            Quantity: ");
            __builder.OpenElement(36, "input");
            __builder.AddAttribute(37, "placeholder", "3 large, 1 can, lots, ...");
            __builder.AddAttribute(38, "value", 
#nullable restore
#line 38 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                             model.Quantity

#line default
#line hidden
#nullable disable
            );
            __builder.AddAttribute(39, "oninput", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, 
#nullable restore
#line 38 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                                       SetQty

#line default
#line hidden
#nullable disable
            ));
            __builder.CloseElement();
            __builder.AddMarkupContent(40, "\r\n");
#nullable restore
#line 39 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
             if (decreaseQty(model).IsSome())
            {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(41, "button");
            __builder.AddAttribute(42, "type", "button");
            __builder.AddAttribute(43, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 40 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                             DecreaseQuantity

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(44, "-");
            __builder.CloseElement();
#nullable restore
#line 40 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                         }

#line default
#line hidden
#nullable disable
#nullable restore
#line 41 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
             if (increaseQty(model).IsSome())
            {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(45, "button");
            __builder.AddAttribute(46, "type", "button");
            __builder.AddAttribute(47, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 42 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                             IncreaseQuantity

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(48, "+");
            __builder.CloseElement();
#nullable restore
#line 42 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                         }

#line default
#line hidden
#nullable disable
            __builder.AddContent(49, "        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(50, "\r\n    ");
            __builder.CloseElement();
            __builder.AddMarkupContent(51, "\r\n    ");
            __builder.OpenElement(52, "div");
            __builder.OpenElement(53, "label");
            __builder.AddContent(54, "Note: ");
            __builder.OpenElement(55, "textarea");
            __builder.AddAttribute(56, "oninput", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, 
#nullable restore
#line 45 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                          OnNoteChange

#line default
#line hidden
#nullable disable
            ));
            __builder.AddAttribute(57, "rows", "3");
            __builder.AddAttribute(58, "placeholder", "Favorite brand, organic-only, ...");
            __builder.AddContent(59, 
#nullable restore
#line 45 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                                                  model.Note

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.CloseElement();
            __builder.CloseElement();
            __builder.AddMarkupContent(60, "\r\n");
#nullable restore
#line 46 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
     if (model.Schedule.IsIncomplete)
    {

#line default
#line hidden
#nullable disable
            __builder.AddContent(61, "        ");
            __builder.OpenElement(62, "div");
            __builder.AddMarkupContent(63, "\r\n            ");
            __builder.OpenElement(64, "label");
            __builder.AddMarkupContent(65, "\r\n                Repeat: <input disabled value=\"No; one-time purchase\">");
#nullable restore
#line 50 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                         if (repeat(model).IsSome())
                {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(66, "button");
            __builder.AddAttribute(67, "type", "button");
            __builder.AddAttribute(68, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 51 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                 OnMakeRecurring

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(69, "Make repeating");
            __builder.CloseElement();
#nullable restore
#line 51 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                         }

#line default
#line hidden
#nullable disable
            __builder.AddContent(70, "            ");
            __builder.CloseElement();
            __builder.AddMarkupContent(71, "\r\n        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(72, "\r\n");
#nullable restore
#line 54 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
    }

#line default
#line hidden
#nullable disable
#nullable restore
#line 55 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
     if (model.Schedule.IsRepeat)
    {
        var repeat = model.Schedule.TryRepeat.Value;

#line default
#line hidden
#nullable disable
            __builder.AddContent(73, "        ");
            __builder.OpenElement(74, "div");
            __builder.AddMarkupContent(75, "\r\n            ");
            __builder.OpenElement(76, "label");
            __builder.AddMarkupContent(77, "\r\n                Repeat: ");
            __builder.OpenElement(78, "select");
            __builder.AddAttribute(79, "onchange", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, 
#nullable restore
#line 60 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                           OnRecurrenceChange

#line default
#line hidden
#nullable disable
            ));
            __builder.AddMarkupContent(80, "\r\n");
#nullable restore
#line 61 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                     foreach (var i in durationChoices)
                    {
                        var d = durationChoices[i.Key].Item2;
                        var isSelected = repeat.Frequency == d;
                        if (isSelected)
                        {

#line default
#line hidden
#nullable disable
            __builder.AddContent(81, "                            ");
            __builder.OpenElement(82, "option");
            __builder.AddAttribute(83, "value", 
#nullable restore
#line 67 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                            i.Key

#line default
#line hidden
#nullable disable
            );
            __builder.AddAttribute(84, "selected", true);
            __builder.AddContent(85, 
#nullable restore
#line 67 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                             i.Value.Item1

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(86, "\r\n");
#nullable restore
#line 68 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                        }
                        else
                        {

#line default
#line hidden
#nullable disable
            __builder.AddContent(87, "                            ");
            __builder.OpenElement(88, "option");
            __builder.AddAttribute(89, "value", 
#nullable restore
#line 71 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                            i.Key

#line default
#line hidden
#nullable disable
            );
            __builder.AddContent(90, 
#nullable restore
#line 71 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                    i.Value.Item1

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(91, "\r\n");
#nullable restore
#line 72 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                        }
                    }

#line default
#line hidden
#nullable disable
            __builder.AddContent(92, "                ");
            __builder.CloseElement();
            __builder.AddMarkupContent(93, "\r\n");
#nullable restore
#line 75 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                 if (removeRepeat(model).IsSome())
                {

#line default
#line hidden
#nullable disable
            __builder.OpenElement(94, "button");
            __builder.AddAttribute(95, "type", "button");
            __builder.AddAttribute(96, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, 
#nullable restore
#line 76 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                 OnRemoveRecurrence

#line default
#line hidden
#nullable disable
            ));
            __builder.AddContent(97, "Remove recurrence");
            __builder.CloseElement();
#nullable restore
#line 76 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                                               }

#line default
#line hidden
#nullable disable
            __builder.AddContent(98, "            ");
            __builder.CloseElement();
            __builder.AddMarkupContent(99, "\r\n        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(100, "\r\n        ");
            __builder.OpenElement(101, "div");
            __builder.AddMarkupContent(102, "\r\n            ");
            __builder.OpenElement(103, "label");
            __builder.AddMarkupContent(104, "\r\n                When:");
#nullable restore
#line 81 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                       
                    void RenderPostponePicker(int? currentDays)
                    {
                        List<Tuple<int, string>> defaultChoices = new List<Tuple<int, string>> {
                Tuple.Create(-1,"On shopping list now"),
                Tuple.Create(0,"Overdue"),
                Tuple.Create(1,"Tomorrow"),
                Tuple.Create(3,"+3 days"),
                Tuple.Create(7,"+1 week"),
                Tuple.Create(14,"+2 weeks"),
                Tuple.Create(21,"+3 weeks"),
                Tuple.Create(30,"+1 month"),
                Tuple.Create(60,"+2 months"),
                Tuple.Create(90,"+3 months")
            };
                        if (!defaultChoices.Any(j => j.Item1 == currentDays) && currentDays.HasValue)
                        {
                            defaultChoices.Add(Tuple.Create(currentDays.Value, $"+{currentDays.Value} days"));
                        }

#line default
#line hidden
#nullable disable
            __builder.AddContent(105, "                        ");
            __builder.OpenElement(106, "select");
            __builder.AddAttribute(107, "onchange", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.ChangeEventArgs>(this, 
#nullable restore
#line 100 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                           Postpone

#line default
#line hidden
#nullable disable
            ));
            __builder.AddMarkupContent(108, "\r\n");
#nullable restore
#line 101 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                             foreach (var i in defaultChoices.OrderBy(j => j.Item1))
                            {
                                if ((!currentDays.HasValue && i.Item1 == -1) || (currentDays == i.Item1))
                                {

#line default
#line hidden
#nullable disable
            __builder.AddContent(109, "                                    ");
            __builder.OpenElement(110, "option");
            __builder.AddAttribute(111, "selected", true);
            __builder.AddAttribute(112, "value", 
#nullable restore
#line 105 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                             i.Item1

#line default
#line hidden
#nullable disable
            );
            __builder.AddContent(113, 
#nullable restore
#line 105 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                                       i.Item2

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(114, "\r\n");
#nullable restore
#line 106 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                }
                                else
                                {

#line default
#line hidden
#nullable disable
            __builder.AddContent(115, "                                    ");
            __builder.OpenElement(116, "option");
            __builder.AddAttribute(117, "value", 
#nullable restore
#line 109 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                    i.Item1

#line default
#line hidden
#nullable disable
            );
            __builder.AddContent(118, 
#nullable restore
#line 109 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                                              i.Item2

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
            __builder.AddMarkupContent(119, "\r\n");
#nullable restore
#line 110 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                                }
                            }

#line default
#line hidden
#nullable disable
            __builder.AddContent(120, "                        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(121, "\r\n");
#nullable restore
#line 113 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
                    }
                    if (repeat.DueDays.IsSome())
                    {
                        RenderPostponePicker(repeat.DueDays.Value);
                    }
                    else
                    {
                        RenderPostponePicker(null);
                    }
                

#line default
#line hidden
#nullable disable
            __builder.AddContent(122, "            ");
            __builder.CloseElement();
            __builder.AddMarkupContent(123, "\r\n        ");
            __builder.CloseElement();
            __builder.AddMarkupContent(124, "\r\n");
#nullable restore
#line 125 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
    }

#line default
#line hidden
#nullable disable
#nullable restore
#line 126 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
     if (canSubmit(model))
    {

#line default
#line hidden
#nullable disable
            __builder.AddContent(125, "        ");
            __builder.AddMarkupContent(126, "<button type=\"button\">Save</button>\r\n");
#nullable restore
#line 129 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
    }
    else
    {

#line default
#line hidden
#nullable disable
            __builder.AddContent(127, "        ");
            __builder.AddMarkupContent(128, "<button disabled>Save</button>\r\n");
#nullable restore
#line 133 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
    }

#line default
#line hidden
#nullable disable
            __builder.AddContent(129, "    ");
            __builder.AddMarkupContent(130, "<button type=\"button\">Cancel</button>\r\n    ");
            __builder.AddMarkupContent(131, "<button type=\"button\">Delete</button>\r\n");
            __builder.CloseElement();
        }
        #pragma warning restore 1998
#nullable restore
#line 137 "C:\Users\justi\source\repos\Groceries\Client\Pages\EditItem.razor"
       

    protected ItemBuilder.Builder model;

    Dictionary<int, Tuple<string, DomainTypes.Duration>> durationChoices;

    protected void Activate() => model = activate(model).Value;

    protected void OnTitleChange(ChangeEventArgs c) => model = setTitle((string)c.Value, model);

    protected void OnNoteChange(ChangeEventArgs c) => model = setNote((string)c.Value, model);

    protected void SetQty(ChangeEventArgs c)
    {
        string qty = (string)c.Value;
        model = ItemBuilder.setQuantity(qty, model);
    }

    protected void IncreaseQuantity() => model = increaseQty(model).Value;

    protected void DecreaseQuantity() => model = decreaseQty(model).Value;

    protected void OnMakeRecurring() => model = repeat(model).Value;

    protected void OnRemoveRecurrence() => model = removeRepeat(model).Value;

    protected void OnRecurrenceChange(ChangeEventArgs c)
    {
        int keyOfChosenItem = int.Parse((string)c.Value);
        var chosen = durationChoices[keyOfChosenItem].Item2;
        model = setFrequency(model).Value.Invoke(chosen);
    }

    protected void Postpone(ChangeEventArgs c)
    {
        int daysChosen = int.Parse((string)c.Value);
        if (daysChosen == -1)
        {
            model = activate(model).Value;
        }
        else
        {
            model = postpone(model).Value.Invoke(daysChosen);
        }
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
        model = ItemBuilder.create;
    }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
