import React, { useEffect, useState } from "react";
import { propertyDetailsStyles as s } from "../../assets/dummyStyles";
import Navbar from "../../components/common/Navbar";
import { useNavigate, useParams, Link } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import axios from "axios";
import API_URL from "../../config";
import {
  HiBadgeCheck,
  HiCalendar,
  HiChatAlt,
  HiChevronLeft,
  HiChevronRight,
  HiCollection,
  HiHeart,
  HiLocationMarker,
  HiOutlineHeart,
  HiOutlineHome,
  HiOutlineUserGroup,
  HiOutlineViewGrid,
  HiX,
} from "react-icons/hi";
import PropertyCard from "../../components/common/PropertyCard";
import { dummyProperties } from "../../assets/dummyProperties";

const PropertyDetails = () => {
  const { id } = useParams();
  const { user, token } = useAuth();
  const navigate = useNavigate();
  const [property, setProperty] = useState(null);
  const [similarProperties, setSimilarProperties] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [inquiry, setInquiry] = useState({
    name: "",
    email: "",
    phone: "",
    message: "",
  });
  const [inquiryStatus, setInquiryStatus] = useState({
    loading: false,
    success: false,
    error: null,
  });
  const [isInWishlist, setIsInWishlist] = useState(false);

  useEffect(() => {
    const fetchDetails = async () => {
      try {
        setLoading(true);
        const res = await axios.get(`${API_URL}/api/property/${id}`, {
          headers: token ? { Authorization: `Bearer ${token}` } : {},
        });

        // لازم يكون فيه property فعلي جوه الرد، وإلا نعتبره فشل ونروح على الـ dummy data
        const fetchedProperty =
          res.data && typeof res.data === "object" && res.data.property
            ? res.data.property
            : null;

        if (!fetchedProperty) {
          throw new Error("Property not found in API response");
        }

        setProperty(fetchedProperty);
        setSimilarProperties(
          Array.isArray(res.data.similarProperties)
            ? res.data.similarProperties
            : [],
        );

        if (user && user.role === "buyer") {
          try {
            const wishRes = await axios.get(`${API_URL}/api/wishlist`, {
              headers: { Authorization: `Bearer ${token}` },
            });
            const found = Array.isArray(wishRes.data)
              ? wishRes.data.some((item) => item.property?._id === id)
              : false;
            setIsInWishlist(found);
          } catch (wishErr) {
            console.warn("Failed to fetch wishlist status:", wishErr);
          }
        }

        setError(null);
      } catch (err) {
        console.warn(
          "No backend available or property not found via API — checking dummy data.",
          err,
        );
        const dummy = dummyProperties.find((p) => p._id === id);

        if (dummy) {
          setProperty(dummy);
          setSimilarProperties(
            dummyProperties.filter(
              (p) =>
                p._id !== dummy._id && p.propertyType === dummy.propertyType,
            ),
          );
          setError(null);
        } else {
          setProperty(null);
          setError("Property not found.");
        }
      } finally {
        setLoading(false);
      }
    };
    fetchDetails();
  }, [id, user, token]);

  //  to handle wishlist toggle
  const handleWishlistToggle = async () => {
    if (!user) return navigate("/login");
    try {
      if (isInWishlist) {
        await axios.delete(`${API_URL}/api/wishlist/${id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        setIsInWishlist(false);
      } else {
        await axios.post(
          `${API_URL}/api/wishlist/${id}`,
          {},
          {
            headers: { Authorization: `Bearer ${token}` },
          },
        );
        setIsInWishlist(true);
      }
    } catch (err) {
      alert("Failed to update wishlist.");
    }
  };
  //to handle inquiry submit
  const handleInquirySubmit = async (e) => {
    e.preventDefault();
    if (!user) return navigate("/login");
    if (user.role !== "buyer") return alert("Only buyers can send inquiries");
    setInquiryStatus({ ...inquiryStatus, loading: true });
    try {
      await axios.post(
        `${API_URL}/api/inquiry`,
        {
          propertyId: id,
          message: inquiry.message,
        },
        { headers: { Authorization: `Bearer ${token}` } },
      );

      setInquiryStatus({ loading: false, success: true, error: null });
      setInquiry({ ...inquiry, message: "" });
    } catch (err) {
      setInquiryStatus({
        loading: false,
        success: false,
        error: "Failed to send inquiry",
      });
    }
  };

  // to start a chat
  const handleChatStart = async () => {
    if (!user) return navigate("/login");
    if (user.role !== "buyer")
      return alert("Only buyers can chat with sellers");

    if (!property?.seller?._id) {
      alert("This is demo data — chat isn't available for it yet.");
      return;
    }

    try {
      const res = await axios.post(
        `${API_URL}/api/chat/start`,
        {
          propertyId: id,
          sellerId: property.seller._id,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        },
      );
      const chat = res.data;

      await axios.post(
        `${API_URL}/api/chat/send`,
        {
          chatId: chat._id,
          text: `(Context: Interested in property "${property.title}")`,
          image: property.images?.[0],
        },
        { headers: { Authorization: `Bearer ${token}` } },
      );
    } catch (err) {
      console.error("Error starting chat:", err);
      alert("Failed to start chat.");
    }
  };
  const [lightboxIndex, setLightboxIndex] = useState(null);

  if (loading)
    return (
      <div className=" loader-full-page">
        <div className="loader"></div>
      </div>
    );
  if (error || !property)
    return (
      <div
        className="container"
        style={{ padding: "4rem", textAlign: "center" }}
      >
        {error || "Property not found"}
      </div>
    );

  const images =
    Array.isArray(property.images) && property.images.length > 0
      ? property.images
      : ["https://via.placeholder.com/800x500?text=No+Image"];

  const formattedPrice = new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 0,
  }).format(property.price || 0);

  const openLightbox = (index) => setLightboxIndex(index);
  const closeLightbox = () => setLightboxIndex(null);
  const nextImage = () =>
    setLightboxIndex((prev) => (prev + 1) % images.length);
  const prevImage = () =>
    setLightboxIndex((prev) => (prev - 1 + images.length) % images.length);

  return (
    <div className={s.pageContainer}>
      <Navbar />
      <main className={s.mainContainer}>
        <nav className={s.breadcrumbs}>
          <Link to="/" className={s.breadcrumbLink}>
            Home
          </Link>
          <HiChevronRight />
          <Link to="/properties" className={s.breadcrumbLink}>
            Listings
          </Link>
          <HiChevronRight />
          <span className={s.breadcrumbCurrent}>{property.title}</span>
        </nav>
        <div className={s.galleryContainer}>
          <div
            className={s.galleryGrid}
            style={{
              gridTemplateColumns:
                images.length > 1 ? "repeat(4, 1fr)" : "1fr",
              gridTemplateRows:
                images.length > 1 ? "repeat(2, 180px)" : "400px",
            }}
          >
            <div
              className={s.galleryMainItem(images.length > 1)}
              onClick={() => openLightbox(0)}
            >
              <img
                src={images[0]}
                alt="property image"
                className={s.galleryImage}
              />
            </div>
            {images.slice(1, 5).map((img, idx) => (
              <div
                key={idx}
                className={s.gallerySideItem}
                onClick={() => openLightbox(idx + 1)}
              >
                <img src={img} alt="image" className={s.galleryImage} />
                {idx === 3 && images.length > 5 && (
                  <div className={s.galleryMoreOverlay}>
                    +{images.length - 5}
                  </div>
                )}
              </div>
            ))}
          </div>
          <div className={s.mobileSliderContainer}>
            <div className={s.mobileSliderTrack}>
              {images.map((img, idx) => (
                <div
                  key={idx}
                  className={s.mobileSlide}
                  onClick={() => openLightbox(idx)}
                >
                  <img src={img} alt="images" className={s.mobileSlideImage} />
                  <div className={s.mobileSlideCounter}>
                    {idx + 1} / {images.length}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
        {/* lightbox model */}
        {lightboxIndex !== null && (
          <div className={s.lightboxOverlay} onClick={closeLightbox}>
            <button onClick={closeLightbox} className={s.lightboxCloseBtn}>
              <HiX size={24} className={s.lightboxCloseIcon} />
            </button>

            <div
              onClick={(e) => e.stopPropagation()}
              className={s.lightboxContent}
            >
              <img
                src={images[lightboxIndex]}
                alt="images"
                className={s.lightboxImage}
              />
              {images.length > 1 && (
                <>
                  <button onClick={prevImage} className={s.lightboxPrevBtn}>
                    <HiChevronLeft size={30} />
                  </button>
                  <button onClick={nextImage} className={s.lightboxNextBtn}>
                    <HiChevronRight size={30} />
                  </button>
                </>
              )}

              <div className={s.lightboxCounter}>
                {lightboxIndex + 1} / {images.length}
              </div>
            </div>
          </div>
        )}
        {/* main content */}
        <div className={s.detailsLayout}>
          <div className={s.infoColumn}>
            <div className={s.infoHeader}>
              <div className={s.titleWrapper}>
                <div className={s.badgeWrapper}>
                  <span className={s.premiumBadge}>Premium Listing</span>
                </div>
                <h1 className={s.propertyTitle}>{property.title}</h1>
                <p className={s.propertyLocation}>
                  <HiLocationMarker className={s.locationIcon} />
                  <span className={s.locationText}>
                    {property.area}, {property.city}, India
                  </span>
                </p>
              </div>
              <div className={s.actionButtons}>
                {(!user || user.role === "buyer") && (
                  <button
                    onClick={handleWishlistToggle}
                    className={s.wishlistButton(isInWishlist)}
                  >
                    {isInWishlist ? (
                      <HiHeart size={26} fill="#ef4444" />
                    ) : (
                      <HiOutlineHeart size={26} />
                    )}
                  </button>
                )}
              </div>
            </div>
            {/* quick stats */}
            <div className={s.statsGrid}>
              {" "}
              {[
                {
                  label: "Bedrooms",
                  value: property.bhk || 0,
                  icon: HiOutlineHome,
                },
                {
                  label: "Bathrooms",
                  value:
                    property.bathrooms ||
                    Math.max(1, (parseInt(property.bhk) || 1) - 1),
                  icon: HiOutlineUserGroup,
                },
                {
                  label: "Furnishing",
                  value: property.furnishing || "N/A",
                  icon: HiCollection,
                },
                {
                  label: "Living Area",
                  value: `${property.areaSize || "—"} sqft`,
                  icon: HiOutlineViewGrid,
                },
                {
                  label: "Type",
                  value: property.propertyType,
                  icon: HiCalendar,
                },
              ].map((stat, i) => (
                <div key={i} className={s.statCard}>
                  {stat.icon && <stat.icon size={18} className={s.statIcon} />}
                  <div className={s.statValue}>{stat.value}</div>
                  <div className={s.statLabel}>{stat.label}</div>
                </div>
              ))}
            </div>
            <div className={s.descriptionSection}>
              <h3 className={s.sectionTitle}>Description</h3>
              <p className={s.descriptionText}>
                {property.description ||
                  "No description available for this property"}
              </p>
            </div>
            <div className={s.amenitiesSection}>
              <h3 className={s.sectionTitle}>Amenities</h3>
              <div className={s.amenitiesGrid}>
                {(property.amenities?.length
                  ? property.amenities
                  : ["Parking", "Security", "Water Supply", "Power Backup"]
                ).map((amn, i) => (
                  <div key={i} className={s.amenityItem}>
                    <HiBadgeCheck size={18} className={s.amenityIcon} />
                    <span className={s.amenityText}>{amn}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
          <div className={s.sidebarColumn}>
            <div
              className={s.priceCard}
              style={{
                background: "var(--primary)",
              }}
            >
              <div className={s.priceCardLabel}>
                {property.status?.toLowerCase() === "rent"
                  ? "Rental Details"
                  : "Listing Price"}
              </div>

              <div className={s.priceCardValue}>
                {property.status?.toLowerCase() === "rent"
                  ? `₹${Number(property.price || 0).toLocaleString("en-IN")}`
                  : formattedPrice}
                {property.status?.toLowerCase() === "rent" && (
                  <span className={s.priceCardPeriod}> /month</span>
                )}
              </div>
              {property.status?.toLowerCase() === "rent" && (
                <div className={s.rentDetails}>
                  <div className={s.rentDetailRow}>
                    <span className={s.rentDetailLabel}>Security Deposit</span>
                    <span className={s.rentDetailValue}>
                      ₹
                      {Number(property.securityDeposit || 0).toLocaleString(
                        "en-IN",
                      )}
                    </span>
                  </div>
                  <div className={s.rentDetailRow}>
                    <span className={s.rentDetailLabel}>Maintenance</span>
                    <span className={s.rentDetailValue}>
                      ₹
                      {Number(property.maintenance || 0).toLocaleString(
                        "en-IN",
                      )}
                      /mo
                    </span>
                  </div>
                </div>
              )}
              <div className={s.priceCardAvailability}>
                Available for{" "}
                {property.status?.toLowerCase() === "rent" ? "Rent" : "Sale"}
              </div>
            </div>
            {/* seller & contact */}
            <div className={s.sellerCard}>
              <div className={s.sellerInfo}>
                <div className={s.sellerAvatar}>
                  <img
                    src={
                      property.seller?.profilePic ||
                      `https://ui-avatars.com/api/?name=${property.seller?.name || "Seller"}&background=0d6e59&color=fff`
                    }
                    alt="Agent"
                    className={s.sellerAvatarImage}
                  />
                </div>
                <div className={s.sellerDetails}>
                  <div className={s.sellerNameLink}>
                    <h4 className={s.sellerName}>
                      {property.seller?.name || "Seller"}
                    </h4>
                  </div>
                  <div className={s.sellerVerifiedBadge}>
                    <HiBadgeCheck className={s.verifiedIcon} /> Verified Seller
                  </div>
                </div>
              </div>

              <div className={s.chatButtonWrapper}>
                <button className={s.chatButton} onClick={handleChatStart}>
                  <HiChatAlt /> Chat
                </button>
              </div>

              {/* Inquiry Form */}
              <h4 className={s.inquiryFormTitle}>Inquire</h4>
              <form onSubmit={handleInquirySubmit}>
                {user?.role === "buyer" ? (
                  <>
                    <textarea
                      placeholder="Your Message..."
                      value={inquiry.message}
                      onChange={(e) =>
                        setInquiry({ ...inquiry, message: e.target.value })
                      }
                      className={s.inquiryTextarea}
                      required
                    />
                    <button
                      type="submit"
                      className={s.inquirySubmitButton}
                      disabled={inquiryStatus.loading}
                    >
                      {inquiryStatus.loading ? "Sending..." : "Send Inquiry"}
                    </button>
                    {inquiryStatus.success && (
                      <p className={s.inquirySuccessMessage}>Inquiry sent!</p>
                    )}
                  </>
                ) : (
                  <div className={s.inquiryDisabledMessage}>
                    <p className={s.inquiryDisabledText}>
                      {user
                        ? "Only buyers can send inquiries."
                        : "Please login as a buyer to send inquiries."}
                    </p>
                    {!user && (
                      <Link to="/login" className={s.inquiryLoginButton}>
                        Login
                      </Link>
                    )}
                  </div>
                )}
              </form>
            </div>
          </div>
        </div>
        <div className={s.additionalDetails}>
          <h3 className={s.detailsTitle}>Property Details</h3>
          <div className={s.detailsGrid}>
            {[
              {
                label: "Property ID",
                value: String(property._id).slice(-8).toUpperCase(),
              },
              {
                label: "Added On",
                value: property.createdAt
                  ? new Date(property.createdAt).toLocaleDateString()
                  : "—",
              },
              { label: "Property Type", value: property.propertyType },
              { label: "Status", value: `For ${property.status || "Sale"}` },
            ].map((detail, i) => (
              <div key={i} className={s.detailRow}>
                <span className={s.detailValue}>{detail.label}</span>
                <span className={s.detailValue}>{detail.value}</span>
              </div>
            ))}
          </div>
        </div>

        <section className={s.similarSection}>
          <div className={s.similarHeader}>
            <div>
              <h2 className={s.similarTitle}>Similar Properties</h2>
              <p className={s.similarSubtitle}>
                Listings you might like in {property.city}.
              </p>
            </div>
            <Link to="/properties" className={s.similarAllLink}>
              All Listings <HiChevronRight />
            </Link>
          </div>
          <div className={s.similarGrid}>
            {similarProperties.length > 0 ? (
              similarProperties
                .slice(0, 3)
                .map((p) => <PropertyCard key={p._id} property={p} />)
            ) : (
              <div className={s.similarEmptyState}>
                No similar properties found in this location.
              </div>
            )}
          </div>
        </section>
      </main>
    </div>
  );
};

export default PropertyDetails;
