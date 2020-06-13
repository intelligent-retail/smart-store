using System;
using SmartRetailApp.Models;

namespace SmartRetailApp.Utilities
{
    public class Mock
    {
        public static CartStatus CreateMockCart(int num)
        {
            return new CartStatus
            {
                User = new User
                {
                    UserName = "SmartStoreUser"
                },
                Cart = new Cart
                {
                    LineItems = new Lineitem[]
                    {
                        new Lineitem
                        {
                            ItemName="DARS",
                            UnitPrice=150.0f,
                            Amount=1,
                            Quantity=1,
                            ImageUrls=new []{ "https://www.morinaga.co.jp/dars/images/item_01_01.png" }
                        }
                    },
                    Taxes = new Tax[]
                    {
                        new Tax{
                            TaxAmount=15.0f
                        }
                    },
                    TotalAmount = 165.0f
                }
            };
        }
    }
}
