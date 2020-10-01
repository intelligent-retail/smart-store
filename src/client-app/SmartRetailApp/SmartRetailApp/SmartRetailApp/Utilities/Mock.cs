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
                            ItemName="Chocolate",
                            UnitPrice=150.0f,
                            Amount=1,
                            Quantity=1,
                            ImageUrls=new []{ "https://github.com/intelligent-retail/smart-store/raw/master/src/arm-template/sample-data/public/item-service/images/testimage.png" }
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
