package com.example.contract.Models.Invoice;

import java.math.BigDecimal;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "invoice_detail")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class InvoiceDetailModel {
    @Id
    @Column(name = "invoice_detail_id", nullable = false)
    private UUID invoiceDetailId;

    @Column(name = "invoice_detail_invoice_id", nullable = false)
    private UUID invoiceDetailInvoiceId;

    @Column(name = "invoice_detail_premise_id", nullable = false)
    private UUID invoiceDetailPremiseId;

    @Column(name = "invoice_detail_rental_price", nullable = false, precision = 18, scale = 2)
    private BigDecimal invoiceDetailRentalPrice;

    @Column(name = "invoice_detail_electricity_fee", nullable = false, precision = 18, scale = 2)
    private BigDecimal invoiceDetailElectricityFee;

    @Column(name = "invoice_detail_water_fee", nullable = false, precision = 18, scale = 2)
    private BigDecimal invoiceDetailWaterFee;

    @Column(name = "invoice_detail_garbage_fee", nullable = false, precision = 18, scale = 2)
    private BigDecimal invoiceDetailGarbageFee;

    @Column(name = "invoice_detail_total_amount", nullable = false, precision = 18, scale = 2)
    private BigDecimal invoiceDetailTotalAmount;
}
