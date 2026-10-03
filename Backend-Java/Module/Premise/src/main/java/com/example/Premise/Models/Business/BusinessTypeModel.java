package com.example.Premise.Models.Business;

import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.UniqueConstraint;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "business_type",
        uniqueConstraints = {
            @UniqueConstraint(
                    name = "uk_business_type_name",
                    columnNames = "business_type_name"
            )
        })
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class BusinessTypeModel {
    @Id
    @Column(name = "business_type_id", nullable = false)
    private UUID businessTypeId;

    @Column(name = "business_type_name", nullable = false, length = 50)
    private String businessTypeName;

    @Column(name = "business_type_description", length = 255)
    private String businessTypeDescription;

    @Column(name = "business_type_is_active", nullable = false)
    private Boolean businessTypeIsActive;

    @Column(name = "business_type_created_at")
    private LocalDateTime businessTypeCreatedAt;

    @Column (name = "business_type_updated_at")
    private LocalDateTime businessTypeUpdatedAt;

}