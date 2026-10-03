package com.example.Premise.Models.Product;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "product_business_type")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder 
public class ProductBusinessTypeModel implements Serializable {
    @EmbeddedId
    private ProductBusinessTypeId id;

    @Column(name = "pbt_assigned_at")
    private LocalDateTime assignedAt;

    @Embeddable
    @Getter
    @Setter
    @NoArgsConstructor
    @AllArgsConstructor
    @EqualsAndHashCode
    public static class ProductBusinessTypeId implements Serializable {
        @Column(name = "pbt_product_id", nullable = false)
        private UUID productId;

        @Column(name = "pbt_business_type_id", nullable = false)
        private UUID businessTypeId;
    }
    
}
