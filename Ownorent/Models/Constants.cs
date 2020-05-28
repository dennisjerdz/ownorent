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
}