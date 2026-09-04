import React from "react";
import { propertyCardStyles as s } from "../../assets/dummyStyles";
import { useAuth } from "../../context/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import {
  HiArrowsExpand,
  HiEye,
  HiHeart,
  HiLocationMarker,
  HiOutlineHeart,
  HiOutlineHome,
  HiOutlineUserGroup,
  HiShieldCheck,
} from "react-icons/hi";
import { formatPrice } from "../../utils/currency";

const FALLBACK_IMAGE =
  "data:image/svg+xml;charset=UTF-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='600' height='400' viewBox='0 0 600 400'%3E%3Crect width='600' height='400' fill='%23e5e7eb'/%3E%3Ctext x='50%25' y='50%25' font-size='24' fill='%239ca3af' text-anchor='middle' dominant-baseline='middle'%3ENo Image%3C/text%3E%3C/svg%3E";

const PropertyCard = ({
  property,
  renderActions,
  isWishlisted,
  onToggleWishlist,
}) => {
  if (!property) return null;

  const { user } = useAuth();
  const navigate = useNavigate();

  const handleWishlistClick = (e) => {
    e.preventDefault();
    e.stopPropagation();

    if (!user) {
      navigate("/login");
      return;
    }
    if (onToggleWishlist) {
      onToggleWishlist(String(property._id));
    }
  };

  const imageUrl =
    Array.isArray(property.images) && property.images.length > 0
      ? property.images[0]
      : property.image || FALLBACK_IMAGE;

  const formattedPrice = formatPrice(property.price);

  const statusBadgeClass = s.badgeStatus
    ? s.badgeStatus(property.status || "sale")
    : "";

  const isCommercial =
    (property.propertyType || "").toLowerCase() === "commercial";

  return (
    <div className={s.card}>
      <Link to={`/property/${property._id}`} className={s.link}>
        <div className={s.imageSection}>
          <img
            src={imageUrl}
            alt={property.title || "Property"}
            className={s.image}
            onError={(e) => {
              e.currentTarget.onerror = null;
              e.currentTarget.src = FALLBACK_IMAGE;
            }}
          />

          {/* top badges */}
          <div className={s.topBadges}>
            <div className={s.badgesLeft}>
              {renderActions ? (
                <span className={statusBadgeClass}>
                  {property.status === "sale" ? "available" : property.status}
                </span>
              ) : (
                <span className={s.badgeNew}>New</span>
              )}
              <span className={s.badgeVerified}>
                <HiShieldCheck size={14} /> Verified
              </span>
              {property.isApproved === false && (
                <span className={s.badgePending}>Pending Review</span>
              )}
            </div>

            {(!user || user.role === "buyer") && (
              <button
                className={s.wishlistButton(isWishlisted)}
                onClick={handleWishlistClick}
              >
                {isWishlisted ? (
                  <HiHeart size={20} />
                ) : (
                  <HiOutlineHeart size={20} />
                )}
              </button>
            )}
          </div>

          <div className={s.priceOverlay}>
            <h3 className={s.price}>{formattedPrice}</h3>
          </div>
        </div>

        <div className={s.content}>
          <div className="flex justify-between items-center">
            <span className={s.propertyType}>
              {property.propertyType || "N/A"}
            </span>
            {property.views !== undefined && (
              <div className={s.views}>
                <HiEye size={16} /> {property.views}
              </div>
            )}
          </div>
          <h4 className={s.title}>{property.title || "Untitled Property"}</h4>
          <div className={s.location}>
            <HiLocationMarker className={s.locationIcon} />
            <span className=" whitespace-nowrap overflow-hidden text-ellipsis">
              {property.area || "—"}, {property.city || "—"}
            </span>
          </div>
          <div className={s.specsGrid}>
            {isCommercial ? (
              <>
                <div className={s.specItem}>
                  <div className={s.specIcon}>
                    <HiOutlineHome size={20} />
                  </div>
                  <div className={s.specValue}>{property.status || "—"}</div>
                  <div className={s.specLabel}>Type</div>
                </div>
                <div className={`${s.specItem} ${s.specDivider}`}>
                  <div className={s.specIcon}>
                    <HiArrowsExpand size={20} />
                  </div>
                  <div className={s.specValue}>{property.areaSize || "—"}</div>
                  <div className={s.specLabel}>Sq Ft</div>
                </div>
                <div className={s.specItem}>
                  <div className={s.specIcon}>
                    <HiShieldCheck size={20} />
                  </div>
                  <div className={s.specValue}>OK</div>
                  <div className={s.specLabel}>Legal</div>
                </div>
              </>
            ) : (
              <>
                <div className={s.specItem}>
                  <div className={s.specIcon}>
                    <HiOutlineHome size={20} />
                  </div>
                  <div className={s.specValue}>{property.bhk ?? "—"}</div>
                  <div className={s.specLabel}>Beds</div>
                </div>
                <div className={`${s.specItem} ${s.specDivider}`}>
                  <div className={s.specIcon}>
                    <HiOutlineUserGroup size={20} />
                  </div>
                  <div className={s.specValue}>
                    {property.bathrooms ||
                      Math.max(1, parseInt(property.bhk) - 1 || 0)}
                  </div>
                  <div className={s.specLabel}>Baths</div>
                </div>
                <div className={s.specItem}>
                  <div className={s.specIcon}>
                    <HiArrowsExpand size={20} />
                  </div>
                  <div className={s.specValue}>{property.areaSize || "—"}</div>
                  <div className={s.specLabel}>Sq Ft</div>
                </div>
              </>
            )}
          </div>

          {!renderActions && (
            <div className={s.viewDetailsButton}>
              <button className={s.viewDetailsBtn}>View Details</button>
            </div>
          )}
        </div>
      </Link>

      {renderActions && (
        <div
          onClick={(e) => {
            e.preventDefault();
            e.stopPropagation();
          }}
          onMouseDown={(e) => e.stopPropagation()}
          className={s.actionsContainer}
        >
          {renderActions(property)}
        </div>
      )}
    </div>
  );
};

export default PropertyCard;
