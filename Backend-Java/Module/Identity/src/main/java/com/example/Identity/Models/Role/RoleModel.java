package com.example.Identity.Models.Role;

import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import jakarta.persistence.Id;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "role")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class RoleModel {
    @Id
    @Column(name = "role_id", nullable=false)
    private UUID roleId;

    @Column(name = "role_code", nullable=false, length=50)
    private String roleCode;

    @Column(name = "role_name", nullable=false, length=150)
    private String roleName;

    @Column(name = "role_description", nullable=false, length=300)
    private String roleDescription;

    @Column(name = "role_is_active", nullable=false)
    private Boolean roleIsActive;

    @Column(name = "role_created_at")
    private LocalDateTime roleCreatedAt;

    @Column(name = "role_updated_at")
    private LocalDateTime roleUpdatedAt;
}
