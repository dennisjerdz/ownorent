using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ownorent.Models
{
    public class AccountTypeConstant {
        public const int ADMIN = 0;
        public const int SELLER = 1;
        public const int CUSTOMER = 2;
        public const int SELLER_COMBO = 3;
    }

    public class AccountStatusConstant {
        public const int PENDING_REQUIREMENTS = 0;
        public const int PENDING_REVIEW = 1;
        public const int REJECTED_RESUBMIT_REQUIREMENTS = 2;
        public const int REJECTED_NOT_ACCEPTING_NEW = 3;
        public const int APPROVED = 4;
        public const int DISABLED = 5;
        public const int REJECTED = 6;
    }

    public class NoteTypeConstant {
        public const int NOTE = 0;
        public const int HISTORY = 1;
        public const int SHIPPING = 2;
    }

    public class ProductTemplateStatusConstant {
        public const int PENDING_WAREHOUSE_ARRIVAL = 0;
        public const int PENDING_REVIEW = 1;
        public const int REJECTED_REVISE = 2;
        public const int REJECTED_NOT_ALLOWED = 3;
        public const int REJECTED_NOT_ALLOWED_RETURNED = 4;
        public const int APPROVED = 5;
        public const int REQUESTED_REMOVAL = 6;
        public const int REMOVED = 7;
    }

    public class ProductStatusConstant {
        public const int AVAILABLE = 0;

        public const int BOUGHT_PAID_PENDING_SELLER = 1;
        public const int BOUGHT_PAID_IN_FREIGHT = 2;
        public const int BOUGHT_PAID = 3;
        public const int BOUGHT_REQUESTED_RETURN = 4;
        public const int BOUGHT_RETURN_APPROVED = 5;
        public const int BOUGHT_RETURNED = 6;

        public const int RENT_PAID_PENDING_SELLER = 7;
        public const int RENT_PAID_IN_FREIGHT = 8;
        public const int RENT_PAID = 9;
        public const int RENT_DONE_RETURN_TO_SELLER = 10;
        public const int RENT_REQUESTED_RETURN = 11;
        public const int RENT_RETURN_APPROVED = 12;
        public const int RENT_RETURNED = 13;
        
        public const int RENT_TO_OWN_PAID_PENDING_SELLER = 14;
        public const int RENT_TO_OWN_PAID_IN_FREIGHT = 15;
        public const int RENT_TO_OWN_PAID = 16;
        public const int RENT_TO_OWN_REQUESTED_RETURN = 17;
        public const int RENT_TO_OWN_RETURN_APPROVED = 18;
        public const int RENT_TO_OWN_RETURNED = 19;

        public const int REQUESTED_REMOVAL = 20;
        public const int REMOVED = 21;
        public const int REMOVED_RETURNED_TO_SELLER = 22;

        public static List<ProductStatusModel> StatusList = new List<ProductStatusModel>()
        {
            new ProductStatusModel() { Value = 0, Type = "BUY", Description = "AVAILABLE" },
            new ProductStatusModel() { Value = 0, Type = "RENT", Description = "AVAILABLE" },
            new ProductStatusModel() { Value = 0, Type = "RENT_TO_OWN", Description = "AVAILABLE" },
            // BUY
            new ProductStatusModel() { Value = 1, Type = "BUY", Description = "BOUGHT_PAID_PENDING_SELLER" },
            new ProductStatusModel() { Value = 2, Type = "BUY", Description = "BOUGHT_PAID_IN_FREIGHT" },
            new ProductStatusModel() { Value = 3, Type = "BUY", Description = "BOUGHT_PAID" },
            new ProductStatusModel() { Value = 4, Type = "BUY", Description = "BOUGHT_REQUESTED_RETURN" },
            new ProductStatusModel() { Value = 5, Type = "BUY", Description = "BOUGHT_RETURN_APPROVED" },
            new ProductStatusModel() { Value = 6, Type = "BUY", Description = "BOUGHT_RETURNED" },
            // RENT
            new ProductStatusModel() { Value = 7, Type = "RENT", Description = "RENT_PAID_PENDING_SELLER" },
            new ProductStatusModel() { Value = 8, Type = "RENT", Description = "RENT_PAID_IN_FREIGHT" },
            new ProductStatusModel() { Value = 9, Type = "RENT", Description = "RENT_PAID" },
            new ProductStatusModel() { Value = 10, Type = "RENT", Description = "RENT_DONE_RETURN_TO_SELLER" },
            new ProductStatusModel() { Value = 11, Type = "RENT", Description = "RENT_REQUESTED_RETURN" },
            new ProductStatusModel() { Value = 12, Type = "RENT", Description = "RENT_RETURN_APPROVED" },
            new ProductStatusModel() { Value = 13, Type = "RENT", Description = "RENT_RETURNED" },
            // RENT TO OWN
            new ProductStatusModel() { Value = 14, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_PAID_PENDING_SELLER" },
            new ProductStatusModel() { Value = 15, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_PAID_IN_FREIGHT" },
            new ProductStatusModel() { Value = 16, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_PAID" },
            new ProductStatusModel() { Value = 17, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_REQUESTED_RETURN" },
            new ProductStatusModel() { Value = 18, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_RETURN_APPROVED" },
            new ProductStatusModel() { Value = 19, Type = "RENT_TO_OWN", Description = "RENT_TO_OWN_RETURNED" },
            // MISC
            new ProductStatusModel() { Value = 20, Type = "MISC", Description = "REQUESTED_REMOVAL" },
            new ProductStatusModel() { Value = 21, Type = "MISC", Description = "REMOVED" },
            new ProductStatusModel() { Value = 22, Type = "MISC", Description = "REMOVED_RETURNED_TO_SELLER" }
        };
    }

    public class ProductStatusModel
    {
        public int Value { get; set; }
        public string Type { get; set; } // BUY, RENT, RENT_TO_OWN, MISC
        public string Description { get; set; }
    }

    public class ProductPriceToUseConstant {
        public const int SELLER_DEFINED_PRICE = 0;
        public const int COMPUTED_PRICE = 1;
        public const int ADMIN_DEFINED_PRICE = 2;
    }

    public class TransactionTypeConstant {
        public const int BUY = 0;
        public const int RENT = 1;
        public const int RENT_TO_OWN = 2;
    }

    public class TransactionStatusConstant {
        public const int PENDING = 0;
        public const int PARTIALLY_PAID = 1;
        public const int PAID = 2;
        public const int CANCEL_REQUESTED = 3;
        public const int CANCELLED = 4;
        public const int CANCELLED_RETURNED = 5;
    }

    public class ShippingStatusConstant
    {
        public const int PENDING = 0;
        public const int REVIEW = 1;
        public const int PACKAGED = 2;
        public const int READY_FOR_PICK_UP = 3;
        public const int IN_TRANSIT = 4;
        public const int DELIVERED = 5;
        public const int FAILED_DELIVERY = 6;
        public const int RETURNED = 7;

        public static List<ShippingStatusModel> StatusList = new List<ShippingStatusModel>()
        {
            new ShippingStatusModel() { Value = 0, Code = "PENDING", Description = "PENDING" },
            new ShippingStatusModel() { Value = 1, Code = "REVIEW", Description = "REVIEW" },
            new ShippingStatusModel() { Value = 2, Code = "PACKAGED", Description = "PACKAGED" },
            new ShippingStatusModel() { Value = 3, Code = "READY_FOR_PICK_UP", Description = "READY FOR PICK UP" },
            new ShippingStatusModel() { Value = 4, Code = "IN_TRANSIT", Description = "IN TRANSIT" },
            new ShippingStatusModel() { Value = 5, Code = "DELIVERED", Description = "DELIVERED" },
            new ShippingStatusModel() { Value = 6, Code = "FAILED_DELIVERY", Description = "FAILED DELIVERY" },
            new ShippingStatusModel() { Value = 7, Code = "RETURNED", Description = "RETURNED" }
        };
    }

    public class ShippingStatusModel
    {
        public int Value { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class ShippingTypeConstant {
        public const int LOCAL = 0;
        public const int PROVINCIAL = 1;
        public const int NONE_ALREADY_DELIVERED = 2; // if payment isn't initial
    }

    public class CartTypeConstant : TransactionTypeConstant { }

    public class TransactionGroupPaymentStatusConstant {
        public const int PENDING = 0;
        public const int SUCCESS = 1;
    }

    public class SellerPayoutStatusConstant
    {
        public const int PENDING = 0;
        public const int REQUESTED = 1;
        public const int REVISE = 2;
        public const int APPROVED = 3;
        public const int SENT = 4;
    }
}