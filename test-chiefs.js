// Simple test to create Kansas City Chiefs
const API_BASE = 'http://localhost:5181';

async function testCreateChiefs() {
    console.log('🏈 Testing Kansas City Chiefs Creation...\n');

    // First, let's create the NFL league
    console.log('📊 Step 1: Creating NFL League...');
    try {
        const leagueResponse = await fetch(`${API_BASE}/League/add`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                name: 'NFL'
            })
        });

        if (leagueResponse.ok) {
            const leagueResult = await leagueResponse.json();
            console.log('✅ League creation result:', leagueResult);
        } else {
            console.log('⚠️ League creation failed, status:', leagueResponse.status);
            const errorText = await leagueResponse.text();
            console.log('Error:', errorText);
        }
    } catch (error) {
        console.log('❌ League creation error:', error.message);
    }

    // Now let's create the Kansas City Chiefs organization
    console.log('\n🏈 Step 2: Creating Kansas City Chiefs...');
    
    const organizationData = {
        name: "Kansas City Chiefs",
        leagueId: "11111111-1111-1111-1111-111111111111", // NFL League ID
        teamId: "KC",
        teamName: "Kansas City Chiefs",
        teamShortName: "Chiefs",
        formedYear: 1960,
        sport: "Football",
        stadium: "Arrowhead Stadium",
        location: "Kansas City, MO",
        stadiumCapacity: 76416,
        website: "https://chiefs.com",
        facebook: "https://facebook.com/chiefs",
        twitter: "https://twitter.com/chiefs",
        instagram: "https://instagram.com/chiefs",
        description: "The Kansas City Chiefs are a professional American football team based in Kansas City, Missouri.",
        color1: "#E31837", // Chiefs Red
        color2: "#FFB81C", // Chiefs Gold
        color3: "#FFFFFF", // White
        badgeUrl: "https://example.com/chiefs-badge.png",
        logoUrl: "https://example.com/chiefs-logo.png",
        fanart1Url: "https://example.com/chiefs-fanart1.jpg",
        fanart2Url: "https://example.com/chiefs-fanart2.jpg",
        fanart3Url: "https://example.com/chiefs-fanart3.jpg"
    };

    try {
        console.log('Sending request to:', `${API_BASE}/Org/addOrganization`);
        console.log('Request data:', JSON.stringify(organizationData, null, 2));
        
        const response = await fetch(`${API_BASE}/Org/addOrganization`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(organizationData)
        });

        console.log(`\nResponse Status: ${response.status} ${response.statusText}`);
        
        if (response.ok) {
            const result = await response.json();
            console.log('✅ Success! Response:', JSON.stringify(result, null, 2));
        } else {
            const errorText = await response.text();
            console.log('❌ Failed! Error response:', errorText);
        }
    } catch (error) {
        console.log('❌ Network Error:', error.message);
    }
}

// Run the test
testCreateChiefs().catch(console.error);
