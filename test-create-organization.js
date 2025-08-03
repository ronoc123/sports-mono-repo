// Test script to create Kansas City Chiefs organization
const API_BASE = 'http://localhost:5181';

async function testCreateOrganization() {
    console.log('🏈 Testing Organization Creation...\n');

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

        const leagueResult = await leagueResponse.json();
        console.log('League creation result:', leagueResult);

        if (!leagueResult.success) {
            console.log('⚠️ League creation failed, but might already exist. Continuing...');
        }
    } catch (error) {
        console.log('❌ League creation error:', error.message);
        return;
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
        const response = await fetch(`${API_BASE}/Org/addOrganization`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(organizationData)
        });

        const result = await response.json();
        
        console.log(`Status: ${response.status} ${response.statusText}`);
        console.log('Response:', JSON.stringify(result, null, 2));

        if (result.success) {
            console.log('✅ Kansas City Chiefs created successfully!');
            console.log(`🆔 Organization ID: ${result.data}`);
            
            // Test getting the organization
            console.log('\n📋 Step 3: Verifying organization was created...');
            await testGetOrganizations();
        } else {
            console.log('❌ Failed to create organization:', result.message);
        }
    } catch (error) {
        console.log('❌ Network Error:', error.message);
    }
}

async function testGetOrganizations() {
    try {
        const response = await fetch(`${API_BASE}/Org/all`);
        const result = await response.json();
        
        if (result.success && result.data && result.data.items) {
            console.log(`✅ Found ${result.data.items.length} organizations:`);
            result.data.items.forEach(org => {
                console.log(`  - ${org.name} (${org.teamShortName}) - ID: ${org.id}`);
            });
        } else {
            console.log('⚠️ No organizations found or unexpected response format');
        }
    } catch (error) {
        console.log('❌ Error getting organizations:', error.message);
    }
}

// Run the test
console.log('🚀 Starting Organization Creation Test...\n');
testCreateOrganization().catch(console.error);
