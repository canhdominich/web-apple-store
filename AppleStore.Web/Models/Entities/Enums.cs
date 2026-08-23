namespace AppleStore.Web.Models.Entities;

public enum OrderStatus { Pending, Confirmed, Shipping, Completed, Cancelled }
public enum PaymentStatus { Pending, Paid, Failed, Refunded, Cancelled }
public enum PaymentMethod { COD, VNPay }
