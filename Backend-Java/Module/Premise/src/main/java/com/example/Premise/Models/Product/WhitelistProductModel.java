package com.example.Premise.Models.Product;

import java.time.LocalDateTime;
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
@Table(name = "whitelist_product")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class WhitelistProductModel {
    @Id 
    @Column(name = "whitelist_product_id", nullable = false)
    private UUID whitelistProductId;

    @Column(name = "whitelist_product_name", nullable = false, length = 100)
    private String whitelistProductName;

    @Column(name = "whitelist_product_description", length = 255)
    private String whitelistProductDescription;

    @Column(name = "whitelist_product_created_at")
    private LocalDateTime whitelistProductCreatedAt;

    @Column(name = "whitelist_product_updated_at")
    private LocalDateTime whitelistProductUpdatedAt;
}
