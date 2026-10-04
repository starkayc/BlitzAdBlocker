namespace BlitzAdBlocker;

// Ad stack observed live in the Blitz app (CDP network capture, v3.0.6):
// Aditude header bidding, Google AdX, prebid bidders, video networks and DMP
// syncs. Keep only exact hosts — blitz.(gg|app.gg) / cdn / analytics are NOT
// ad domains and stay unblocked.
internal static class BlockedDomains
{
    public static readonly string[] List =
    {
        // ad server / header bidding: Aditude
        "aditude.io", "www.aditude.io", "edge.aditude.io", "raven-edge.aditude.io",
        "raven-static.aditude.io", "cw-static.aditude.io", "geo.aditude.io",
        "aditude.cloud", "match.aditude.cloud", "event-ingestor.judy.pnap.aditude.cloud",
        "dn0qt3r0xannq.cloudfront.net",
        // Google AdX (GPT)
        "doubleclick.net", "www.doubleclick.net", "securepubads.g.doubleclick.net",
        "cm.g.doubleclick.net", "adservice.google.com",
        // video ads / RTB
        "kueezrtb.com", "www.kueezrtb.com", "static.kueezrtb.com",
        "track.kueezrtb.com", "gtrack.kueezrtb.com",
        // prebid + DMP syncs
        "quantserve.com", "www.quantserve.com", "pixel.quantserve.com",
        "secure.quantserve.com", "quantcount.com", "rules.quantcount.com",
        "rlcdn.com", "www.rlcdn.com", "idsync.rlcdn.com", "id.rlcdn.com",
        "criteo.com", "gum.criteo.com",
        "amazon-adsystem.com", "c.amazon-adsystem.com", "config.aps.amazon-adsystem.com",
        "id5-sync.com", "www.id5-sync.com", "api.id5-sync.com", "cdn.id5-sync.com",
        "ad.gt", "www.ad.gt", "a.ad.gt", "p.ad.gt", "id.hadron.ad.gt", "ids.ad.gt",
        "ids4.ad.gt", "pixels.ad.gt",
        "openwebmp.com", "cs.openwebmp.com",
        "bidswitch.net", "x.bidswitch.net",
        "3lift.com", "eb2.3lift.com",
        "crwdcntrl.net", "id.crwdcntrl.net", "tags.crwdcntrl.net",
        "omnitagjs.com", "visitor.omnitagjs.com",
        "a-mo.net", "c3.a-mo.net", "sync.a-mo.net",
        "fastclick.net", "secure.cdn.fastclick.net",
        "33across.com", "cdn-ima.33across.com",
        "hadronid.net", "cdn.hadronid.net",
        "ipredictive.com", "ad.ipredictive.com",
        "fwmrm.net", "user-sync.fwmrm.net",
        "optable.co", "us.edge.optable.co", "solutions.cdn.optable.co",
        "everesttech.net", "rtd-tm.everesttech.net",
        "loc.kr", "identity.loc.kr", "aim.loc.kr",
        "inmobi.com", "cmp.inmobi.com",
        "privacymanager.io", "launchpad.privacymanager.io",
        "launchpad-wrapper.privacymanager.io",
        "prebid.cloud", "geo-location.prebid.cloud",
        "script.ac", "cadmus.script.ac",
        "rapidedge.io", "metrics.rapidedge.io",
        // high-volume prebid bidders / video seen in app traffic
        "adnxs.com", "secure.adnxs.com", "ib.adnxs.com",
        "pubmatic.com", "hbopenbid.pubmatic.com", "st.pubmatic.com",
        "image4.pubmatic.com", "image8.pubmatic.com", "feed-notif.pubmatic.com",
        "openx.net", "rtb.openx.net", "solomid-d.openx.net",
        "adsrvr.org", "direct.adsrvr.org", "match.adsrvr.org",
        "enduser.adsrvr.org", "v.adsrvr.org", "ny4-bid.adsrvr.org",
        "casalemedia.com", "dsum.casalemedia.com", "dsum-sec.casalemedia.com",
        "htlb.casalemedia.com", "ced-ns.sascdn.com",
        "smartadserver.com", "csync-us.smartadserver.com",
        "csync-global.smartadserver.com", "prg.smartadserver.com",
        "rubiconproject.com", "prebid-server.rubiconproject.com",
        "primis.tech", "pl.primis.tech", "live.primis.tech", "rtb.primis.tech",
        "viralize.tv", "ads.viralize.tv", "monetize-static.viralize.tv",
        "s2s.viralize.tv",
        "360yield.com", "ad.360yield.com", "nexverse.ai", "rtb.nexverse.ai",
        "blismedia.com", "tr.blismedia.com", "kargo.com", "krk2.kargo.com",
        "admanmedia.com", "cs.admanmedia.com", "adkernel.com", "sync.adkernel.com",
        "1rx.io", "sync.1rx.io", "resetdigital.co", "sync.resetdigital.co",
        "rtbwise.com", "trace.us.rtbwise.com", "vindicosuite.com", "x.vindicosuite.com",
        "activemetering.com", "track.activemetering.com", "iionads.com",
        "tracker.iionads.com", "unrulymedia.com", "targeting.unrulymedia.com",
        "doubleverify.com", "vast.doubleverify.com", "tpsc-video-ue.doubleverify.com",
        "2mdn.net", "gcdn.2mdn.net", "googlesyndication.com", "ade.googlesyndication.com",
        "contextweb.com", "bh.contextweb.com", "intentiq.com", "agent.intentiq.com",
        "reports.intentiq.com", "nexx360.io", "fast.nexx360.io",
        "dxkulture.com", "ads.dxkulture.com", "servenobid.com", "ads.servenobid.com",
        "prod.bidr.io", "match.prod.bidr.io", "bidswitch.net",
        "avocet.io", "vtrk.dv.tech", "dv.tech",
    };
}